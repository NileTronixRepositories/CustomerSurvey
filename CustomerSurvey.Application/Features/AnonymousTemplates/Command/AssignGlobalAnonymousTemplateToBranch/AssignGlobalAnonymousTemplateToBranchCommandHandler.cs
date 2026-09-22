using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;

internal sealed class AssignGlobalAnonymousTemplateToBranchCommandHandler
    : ICommandHandler<AssignGlobalAnonymousTemplateToBranchCommand, AssignGlobalAnonymousTemplateToBranchResponse>
{
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
    private readonly IWriteRepository<AnonymousTemplate> _templateWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestion> _questionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _conditionWriteRepository;
    private readonly IPublicSurveyUrlBuilder _publicSurveyUrlBuilder;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly IMediaService _mediaService;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AssignGlobalAnonymousTemplateToBranchCommandHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<AnonymousTemplate> templateReadRepository,
        IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
        IWriteRepository<AnonymousTemplate> templateWriteRepository,
        IWriteRepository<AnonymousTemplateQuestion> questionWriteRepository,
        IWriteRepository<AnonymousTemplateQuestionCondition> conditionWriteRepository,
        IPublicSurveyUrlBuilder publicSurveyUrlBuilder,
        IQrCodeGenerator qrCodeGenerator,
        IMediaService mediaService,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _superAdminReadRepository = superAdminReadRepository;
        _branchReadRepository = branchReadRepository;
        _templateReadRepository = templateReadRepository;
        _conditionReadRepository = conditionReadRepository;
        _templateWriteRepository = templateWriteRepository;
        _questionWriteRepository = questionWriteRepository;
        _conditionWriteRepository = conditionWriteRepository;
        _publicSurveyUrlBuilder = publicSurveyUrlBuilder;
        _qrCodeGenerator = qrCodeGenerator;
        _mediaService = mediaService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AssignGlobalAnonymousTemplateToBranchResponse>> Handle(
        AssignGlobalAnonymousTemplateToBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Fail("AnonymousTemplates.AssignGlobal.Unauthenticated", "Authentication is required.", ErrorType.Security);
        }

        var userId = _currentUser.UserId.Value;
        var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == userId,
            cancellationToken);

        if (!isSuperAdmin)
        {
            return Fail("AnonymousTemplates.AssignGlobal.Forbidden", "Only a SuperAdmin can assign a global anonymous template.", ErrorType.Security);
        }

        var source = await _templateReadRepository.FirstOrDefaultAsync(
            new GetGlobalAnonymousTemplateForAssignmentSpec(request.GlobalTemplateId),
            cancellationToken);

        if (source is null || !source.IsGlobal)
        {
            return Fail("AnonymousTemplates.AssignGlobal.SourceNotFound", "Global anonymous template was not found.", ErrorType.NotFound);
        }

        if (source.IsArchived)
        {
            return Fail("AnonymousTemplates.AssignGlobal.SourceArchived", "The global anonymous template must be restored before assignment.", ErrorType.Validation);
        }

        var branch = await _branchReadRepository.GetByPropertyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (branch is null)
        {
            return Fail("AnonymousTemplates.AssignGlobal.BranchNotFound", "Target branch was not found.", ErrorType.NotFound);
        }

        if (!branch.IsActive)
        {
            return Fail("AnonymousTemplates.AssignGlobal.BranchInactive", "Target branch is inactive.", ErrorType.Validation);
        }

        var alreadyAssigned = await _templateReadRepository.AnyAsync(
            x => x.BranchId == request.BranchId &&
                 x.SourceGlobalAnonymousTemplateId == source.Id,
            cancellationToken);

        if (alreadyAssigned)
        {
            return Fail("AnonymousTemplates.AssignGlobal.Duplicate", "This global anonymous template is already assigned to the target branch.", ErrorType.Conflict);
        }

        var nameConflict = await _templateReadRepository.AnyAsync(
            x => x.Scope == AnonymousTemplateScope.Branch &&
                 x.BranchId == request.BranchId &&
                 x.NameEn == source.NameEn,
            cancellationToken);

        if (nameConflict)
        {
            return Fail("AnonymousTemplates.AssignGlobal.NameConflict", "An anonymous template with the same name already exists in the target branch.", ErrorType.Conflict);
        }

        var target = AnonymousTemplate.CreateBranchTemplate(
            request.BranchId,
            source.NameEn,
            source.NameAr,
            source.Description,
            request.ActiveFrom,
            request.ExpireTo,
            userId,
            sourceGlobalAnonymousTemplateId: source.Id);

        foreach (var input in source.CustomInputs.Where(x => x.IsActive).OrderBy(x => x.Order))
        {
            target.AddCustomInput(AnonymousTemplateCustomInput.Create(
                target.Id, input.Name, input.LabelEn, input.LabelAr, input.Type,
                input.IsRequired, input.MinLength, input.MaxLength, input.MinValue,
                input.MaxValue, input.StartWith, input.Order, userId));
        }

        var targetQuestions = source.Questions
            .OrderBy(x => x.Order)
            .Select(x => AnonymousTemplateQuestion.Create(target.Id, x.QuestionId, x.Order, userId))
            .ToArray();

        var questionIdMap = source.Questions
            .OrderBy(x => x.Order)
            .Zip(targetQuestions)
            .ToDictionary(x => x.First.Id, x => x.Second.Id);

        var sourceConditions = await _conditionReadRepository.ListAsync(
            new GetGlobalAnonymousTemplateConditionsForAssignmentSpec(source.Id),
            cancellationToken);

        var targetConditions = sourceConditions
            .Select(x => CopyCondition(target.Id, x, questionIdMap, userId))
            .ToArray();

        var publicUrl = _publicSurveyUrlBuilder.BuildAnonymousTemplateUrl(target.Id);
        target.SetPublicAccess(publicUrl, _qrCodeGenerator.GenerateBase64Png(publicUrl));

        string? savedLogoPath = null;
        if (request.Logo is not null)
        {
            var validationError = TemplateLogoMedia.Validate(request.Logo);
            if (validationError is not null)
            {
                return Result<AssignGlobalAnonymousTemplateToBranchResponse>.Fail(validationError);
            }

            var fileName = await _mediaService.SaveAsync(request.Logo, FileNames.TemplateLogos);
            savedLogoPath = TemplateLogoMedia.ToRelativePath(fileName);
            target.SetLogoPath(savedLogoPath);
        }

        try
        {
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
            if (savedLogoPath is not null)
            {
                _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(savedLogoPath));
            }
            throw;
        }

        return Result<AssignGlobalAnonymousTemplateToBranchResponse>.Ok(new()
        {
            AnonymousTemplateId = target.Id,
            SourceGlobalAnonymousTemplateId = source.Id,
            BranchId = branch.Id,
            BranchNameEn = branch.NameEn,
            BranchNameAr = branch.NameAr,
            NameEn = target.NameEn,
            NameAr = target.NameAr,
            ActiveFrom = target.ActiveFrom,
            ExpireTo = target.ExpireTo,
            IsActive = target.IsActive,
            PublicUrl = target.PublicUrl,
            QrCode = target.QrCode,
            LogoPath = target.LogoPath,
            QuestionsCount = targetQuestions.Length,
            ConditionsCount = targetConditions.Length,
            CustomInputsCount = target.CustomInputs.Count
        });
    }

    private static AnonymousTemplateQuestionCondition CopyCondition(
        Guid templateId,
        AnonymousTemplateQuestionCondition source,
        IReadOnlyDictionary<Guid, Guid> questionIdMap,
        Guid userId)
        => source.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                AnonymousTemplateQuestionCondition.CreateForSingleChoice(
                    templateId, questionIdMap[source.ParentAnonymousTemplateQuestionId],
                    questionIdMap[source.ChildAnonymousTemplateQuestionId],
                    source.SelectedQuestionOptionId!.Value, source.Order, userId),
            QuestionConditionTriggerType.StarRatingValue =>
                AnonymousTemplateQuestionCondition.CreateForStarRating(
                    templateId, questionIdMap[source.ParentAnonymousTemplateQuestionId],
                    questionIdMap[source.ChildAnonymousTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            QuestionConditionTriggerType.SmileValue =>
                AnonymousTemplateQuestionCondition.CreateForSmiles(
                    templateId, questionIdMap[source.ParentAnonymousTemplateQuestionId],
                    questionIdMap[source.ChildAnonymousTemplateQuestionId],
                    source.TriggerValue!.Value, source.Order, userId),
            _ => throw new InvalidOperationException("Unsupported anonymous template condition trigger type.")
        };

    private static Result<AssignGlobalAnonymousTemplateToBranchResponse> Fail(
        string code, string message, ErrorType type)
        => Result<AssignGlobalAnonymousTemplateToBranchResponse>.Fail(new Error(code, message, type));
}
