using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;

internal sealed class CopyTemplateAsAnonymousCommandHandler
    : ICommandHandler<CopyTemplateAsAnonymousCommand, CopyTemplateAsAnonymousResponse>
{
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly IWriteRepository<AnonymousTemplate> _anonymousWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestion> _questionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _conditionWriteRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly IPublicSurveyUrlBuilder _urlBuilder;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public CopyTemplateAsAnonymousCommandHandler(
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
        IWriteRepository<Template> templateWriteRepository,
        IWriteRepository<AnonymousTemplate> anonymousWriteRepository,
        IWriteRepository<AnonymousTemplateQuestion> questionWriteRepository,
        IWriteRepository<AnonymousTemplateQuestionCondition> conditionWriteRepository,
        ICurrentBranchScopeResolver branchScopeResolver,
        IPublicSurveyUrlBuilder urlBuilder,
        IQrCodeGenerator qrCodeGenerator,
        IMediaService mediaService,
        IUnitOfWork unitOfWork)
    {
        _templateReadRepository = templateReadRepository;
        _anonymousReadRepository = anonymousReadRepository;
        _branchReadRepository = branchReadRepository;
        _conditionReadRepository = conditionReadRepository;
        _templateWriteRepository = templateWriteRepository;
        _anonymousWriteRepository = anonymousWriteRepository;
        _questionWriteRepository = questionWriteRepository;
        _conditionWriteRepository = conditionWriteRepository;
        _branchScopeResolver = branchScopeResolver;
        _urlBuilder = urlBuilder;
        _qrCodeGenerator = qrCodeGenerator;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CopyTemplateAsAnonymousResponse>> Handle(
        CopyTemplateAsAnonymousCommand request,
        CancellationToken cancellationToken)
    {
        var scope = await _branchScopeResolver.ResolveAsync(cancellationToken);
        if (scope.IsFailure)
        {
            return Result<CopyTemplateAsAnonymousResponse>.Fail(scope.Errors);
        }

        var source = await _templateReadRepository.FirstOrDefaultAsync(
            new GetTemplateForCopyAsAnonymousSpec(request.TemplateId, scope.Value.BranchId),
            cancellationToken);

        if (source is null)
        {
            return Fail("Templates.CopyAsAnonymous.SourceNotFound", "Template was not found in the current branch.", ErrorType.NotFound);
        }

        var branch = await _branchReadRepository.GetByPropertyAsync(
            x => x.Id == scope.Value.BranchId,
            cancellationToken);

        if (!source.IsActive)
        {
            return Fail("Templates.CopyAsAnonymous.SourceInactive", "The source template is inactive.", ErrorType.Validation);
        }

        var nameConflict = await _anonymousReadRepository.AnyAsync(
            x => x.Scope == AnonymousTemplateScope.Branch &&
                 x.BranchId == source.BranchId &&
                 x.NameEn == source.NameEn,
            cancellationToken);

        if (nameConflict)
        {
            return Fail("Templates.CopyAsAnonymous.NameConflict", "An anonymous template with the same name already exists in this branch.", ErrorType.Conflict);
        }

        var familyId = source.TemplateFamilyId ?? Guid.NewGuid();
        source.SetTemplateFamily(familyId);

        var target = AnonymousTemplate.CreateBranchTemplate(
            source.BranchId, source.NameEn, source.NameAr, source.Description,
            source.ActiveFrom, source.ExpireTo, scope.Value.ApplicationUserId,
            templateFamilyId: familyId);

        var publicUrl = _urlBuilder.BuildAnonymousTemplateUrl(target.Id);
        target.SetPublicAccess(publicUrl, _qrCodeGenerator.GenerateBase64Png(publicUrl));

        var targetQuestions = source.TemplateQuestions
            .OrderBy(x => x.Order)
            .Select(x => AnonymousTemplateQuestion.Create(target.Id, x.QuestionId, x.Order, scope.Value.ApplicationUserId))
            .ToArray();

        var idMap = source.TemplateQuestions
            .OrderBy(x => x.Order)
            .Zip(targetQuestions)
            .ToDictionary(x => x.First.Id, x => x.Second.Id);

        var sourceConditions = await _conditionReadRepository.ListAsync(
            new GetTemplateConditionsForCopyAsAnonymousSpec(source.Id),
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
            _templateWriteRepository.Update(source);
            await _anonymousWriteRepository.AddAsync(target, cancellationToken);
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

        return Result<CopyTemplateAsAnonymousResponse>.Ok(new()
        {
            SourceTemplateId = source.Id,
            AnonymousTemplateId = target.Id,
            BranchId = source.BranchId,
            BranchNameEn = branch?.NameEn ?? string.Empty,
            BranchNameAr = branch?.NameAr,
            TemplateFamilyId = familyId,
            NameEn = target.NameEn,
            NameAr = target.NameAr,
            LogoPath = target.LogoPath,
            PublicUrl = target.PublicUrl,
            QrCode = target.QrCode,
            QuestionsCount = targetQuestions.Length,
            ConditionsCount = targetConditions.Length
        });
    }

    private static AnonymousTemplateQuestionCondition CopyCondition(
        Guid targetId, TemplateQuestionCondition source,
        IReadOnlyDictionary<Guid, Guid> idMap, Guid userId)
        => source.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                AnonymousTemplateQuestionCondition.CreateForSingleChoice(targetId,
                    idMap[source.ParentTemplateQuestionId], idMap[source.ChildTemplateQuestionId],
                    source.SelectedQuestionOptionId!.Value, source.Order, userId),
            QuestionConditionTriggerType.StarRatingValue =>
                AnonymousTemplateQuestionCondition.CreateForStarRating(targetId,
                    idMap[source.ParentTemplateQuestionId], idMap[source.ChildTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            QuestionConditionTriggerType.SmileValue =>
                AnonymousTemplateQuestionCondition.CreateForSmiles(targetId,
                    idMap[source.ParentTemplateQuestionId], idMap[source.ChildTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            _ => throw new InvalidOperationException("Unsupported template condition trigger type.")
        };

    private static Result<CopyTemplateAsAnonymousResponse> Fail(string code, string message, ErrorType type)
        => Result<CopyTemplateAsAnonymousResponse>.Fail(new Error(code, message, type));
}
