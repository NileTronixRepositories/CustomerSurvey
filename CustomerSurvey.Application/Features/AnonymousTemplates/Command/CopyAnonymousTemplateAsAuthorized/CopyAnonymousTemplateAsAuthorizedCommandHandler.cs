using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;

internal sealed class CopyAnonymousTemplateAsAuthorizedCommandHandler
    : ICommandHandler<CopyAnonymousTemplateAsAuthorizedCommand, CopyAnonymousTemplateAsAuthorizedResponse>
{
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
    private readonly IWriteRepository<AnonymousTemplate> _anonymousWriteRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly IWriteRepository<TemplateQuestion> _questionWriteRepository;
    private readonly IWriteRepository<TemplateQuestionCondition> _conditionWriteRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public CopyAnonymousTemplateAsAuthorizedCommandHandler(
        IWriteReadRepository<AnonymousTemplate> anonymousReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
        IWriteRepository<AnonymousTemplate> anonymousWriteRepository,
        IWriteRepository<Template> templateWriteRepository,
        IWriteRepository<TemplateQuestion> questionWriteRepository,
        IWriteRepository<TemplateQuestionCondition> conditionWriteRepository,
        ICurrentBranchScopeResolver branchScopeResolver,
        IMediaService mediaService,
        IUnitOfWork unitOfWork)
    {
        _anonymousReadRepository = anonymousReadRepository;
        _templateReadRepository = templateReadRepository;
        _branchReadRepository = branchReadRepository;
        _conditionReadRepository = conditionReadRepository;
        _anonymousWriteRepository = anonymousWriteRepository;
        _templateWriteRepository = templateWriteRepository;
        _questionWriteRepository = questionWriteRepository;
        _conditionWriteRepository = conditionWriteRepository;
        _branchScopeResolver = branchScopeResolver;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CopyAnonymousTemplateAsAuthorizedResponse>> Handle(
        CopyAnonymousTemplateAsAuthorizedCommand request,
        CancellationToken cancellationToken)
    {
        var scope = await _branchScopeResolver.ResolveAsync(cancellationToken);
        if (scope.IsFailure)
        {
            return Result<CopyAnonymousTemplateAsAuthorizedResponse>.Fail(scope.Errors);
        }

        var source = await _anonymousReadRepository.FirstOrDefaultAsync(
            new GetAnonymousTemplateForCopyAsAuthorizedSpec(request.AnonymousTemplateId, scope.Value.BranchId),
            cancellationToken);

        if (source is null || !source.IsBranchScoped)
        {
            return Fail("AnonymousTemplates.CopyAsAuthorized.SourceNotFound", "Anonymous template was not found in the current branch.", ErrorType.NotFound);
        }

        var branch = await _branchReadRepository.GetByPropertyAsync(
            x => x.Id == scope.Value.BranchId,
            cancellationToken);

        if (!source.IsActive)
        {
            return Fail("AnonymousTemplates.CopyAsAuthorized.SourceInactive", "The source anonymous template is inactive.", ErrorType.Validation);
        }

        if (source.SourceGlobalAnonymousTemplateId.HasValue)
        {
            return Fail("AnonymousTemplates.CopyAsAuthorized.ManagedCopyForbidden", "A managed global branch copy cannot be converted to an authorized template.", ErrorType.Validation);
        }

        var nameConflict = await _templateReadRepository.AnyAsync(
            x => x.BranchId == source.BranchId && x.NameEn == source.NameEn,
            cancellationToken);

        if (nameConflict)
        {
            return Fail("AnonymousTemplates.CopyAsAuthorized.NameConflict", "An authorized template with the same name already exists in this branch.", ErrorType.Conflict);
        }

        var familyId = source.TemplateFamilyId ?? Guid.NewGuid();
        source.SetTemplateFamily(familyId);

        var target = Template.Create(
            source.BranchId!.Value, source.NameEn, source.NameAr, source.Description,
            source.ActiveFrom, source.ExpireTo, scope.Value.ApplicationUserId,
            templateFamilyId: familyId);

        var targetQuestions = source.Questions
            .OrderBy(x => x.Order)
            .Select(x => TemplateQuestion.Create(target.Id, x.QuestionId, x.Order, scope.Value.ApplicationUserId))
            .ToArray();

        var idMap = source.Questions
            .OrderBy(x => x.Order)
            .Zip(targetQuestions)
            .ToDictionary(x => x.First.Id, x => x.Second.Id);

        var sourceConditions = await _conditionReadRepository.ListAsync(
            new GetAnonymousConditionsForCopyAsAuthorizedSpec(source.Id),
            cancellationToken);

        var targetConditions = sourceConditions
            .Select(x => CopyCondition(target.Id, x, idMap, scope.Value.ApplicationUserId))
            .ToArray();

        string? copiedLogoPath = null;
        if (!string.IsNullOrWhiteSpace(source.LogoPath))
        {
            var fileName = await _mediaService.CopyAsync(source.LogoPath, FileNames.TemplateLogos);
            copiedLogoPath = TemplateLogoMedia.ToRelativePath(fileName);
            target.SetLogoPath(copiedLogoPath);
        }

        try
        {
            _anonymousWriteRepository.Update(source);
            await _templateWriteRepository.AddAsync(target, cancellationToken);
            if (targetQuestions.Length > 0)
            {
                await _questionWriteRepository.AddRangeAsync(targetQuestions.ToList(), cancellationToken);
            }
            if (targetConditions.Length > 0)
            {
                await _conditionWriteRepository.AddRangeAsync(targetConditions.ToList(), cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (copiedLogoPath is not null)
            {
                _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(copiedLogoPath));
            }
            throw;
        }

        return Result<CopyAnonymousTemplateAsAuthorizedResponse>.Ok(new()
        {
            SourceAnonymousTemplateId = source.Id,
            TemplateId = target.Id,
            BranchId = target.BranchId,
            BranchNameEn = branch?.NameEn ?? string.Empty,
            BranchNameAr = branch?.NameAr,
            TemplateFamilyId = familyId,
            NameEn = target.NameEn,
            NameAr = target.NameAr,
            LogoPath = target.LogoPath,
            QuestionsCount = targetQuestions.Length,
            ConditionsCount = targetConditions.Length
        });
    }

    private static TemplateQuestionCondition CopyCondition(
        Guid targetId, AnonymousTemplateQuestionCondition source,
        IReadOnlyDictionary<Guid, Guid> idMap, Guid userId)
        => source.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                TemplateQuestionCondition.CreateForSingleChoice(targetId,
                    idMap[source.ParentAnonymousTemplateQuestionId], idMap[source.ChildAnonymousTemplateQuestionId],
                    source.SelectedQuestionOptionId!.Value, source.Order, userId),
            QuestionConditionTriggerType.StarRatingValue =>
                TemplateQuestionCondition.CreateForStarRating(targetId,
                    idMap[source.ParentAnonymousTemplateQuestionId], idMap[source.ChildAnonymousTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            QuestionConditionTriggerType.SmileValue =>
                TemplateQuestionCondition.CreateForSmiles(targetId,
                    idMap[source.ParentAnonymousTemplateQuestionId], idMap[source.ChildAnonymousTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            _ => throw new InvalidOperationException("Unsupported anonymous template condition trigger type.")
        };

    private static Result<CopyAnonymousTemplateAsAuthorizedResponse> Fail(string code, string message, ErrorType type)
        => Result<CopyAnonymousTemplateAsAuthorizedResponse>.Fail(new Error(code, message, type));
}
