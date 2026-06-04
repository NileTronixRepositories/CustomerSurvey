using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class CopyTemplateToBranchCommandHandler
    : ICommandHandler<CopyTemplateToBranchCommand, CopyTemplateToBranchResponse>
{
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<TemplateQuestionCondition> _templateConditionReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateConditionReadRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly IWriteRepository<TemplateQuestion> _templateQuestionWriteRepository;
    private readonly IWriteRepository<TemplateQuestionCondition> _templateConditionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateConditionWriteRepository;
    private readonly IPublicSurveyUrlBuilder _publicSurveyUrlBuilder;
    private readonly IQrCodeGenerator _qrCodeGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CopyTemplateToBranchCommandHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<TemplateQuestionCondition> templateConditionReadRepository,
        IWriteReadRepository<AnonymousTemplateQuestionCondition> anonymousTemplateConditionReadRepository,
        IWriteRepository<Template> templateWriteRepository,
        IWriteRepository<TemplateQuestion> templateQuestionWriteRepository,
        IWriteRepository<TemplateQuestionCondition> templateConditionWriteRepository,
        IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
        IWriteRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionWriteRepository,
        IWriteRepository<AnonymousTemplateQuestionCondition> anonymousTemplateConditionWriteRepository,
        IPublicSurveyUrlBuilder publicSurveyUrlBuilder,
        IQrCodeGenerator qrCodeGenerator,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));
        _templateConditionReadRepository = templateConditionReadRepository
            ?? throw new ArgumentNullException(nameof(templateConditionReadRepository));
        _anonymousTemplateConditionReadRepository = anonymousTemplateConditionReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateConditionReadRepository));
        _templateWriteRepository = templateWriteRepository
            ?? throw new ArgumentNullException(nameof(templateWriteRepository));
        _templateQuestionWriteRepository = templateQuestionWriteRepository
            ?? throw new ArgumentNullException(nameof(templateQuestionWriteRepository));
        _templateConditionWriteRepository = templateConditionWriteRepository
            ?? throw new ArgumentNullException(nameof(templateConditionWriteRepository));
        _anonymousTemplateWriteRepository = anonymousTemplateWriteRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateWriteRepository));
        _anonymousTemplateQuestionWriteRepository = anonymousTemplateQuestionWriteRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionWriteRepository));
        _anonymousTemplateConditionWriteRepository = anonymousTemplateConditionWriteRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateConditionWriteRepository));
        _publicSurveyUrlBuilder = publicSurveyUrlBuilder
            ?? throw new ArgumentNullException(nameof(publicSurveyUrlBuilder));
        _qrCodeGenerator = qrCodeGenerator
            ?? throw new ArgumentNullException(nameof(qrCodeGenerator));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _unitOfWork = unitOfWork
            ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<CopyTemplateToBranchResponse>> Handle(
        CopyTemplateToBranchCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentApplicationUserId = _currentUser.UserId.Value;

        var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == currentApplicationUserId,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.CurrentSuperAdminNotFound",
                Message: ErrorMessage.CopyTemplateToBranch_CurrentSuperAdmin_NotFound,
                Type: ErrorType.Security));
        }

        var branchExists = await _branchReadRepository.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (!branchExists)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.BranchNotFound",
                Message: ErrorMessage.CopyTemplateToBranch_Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        var sourceTemplate = await _templateReadRepository.FirstOrDefaultAsync(
            new GetAuthorizedTemplateForCopyToBranchSpec(request.TemplateId),
            cancellationToken);

        var sourceAnonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
            new GetAnonymousTemplateForCopyToBranchSpec(request.TemplateId),
            cancellationToken);

        if (sourceTemplate is not null && sourceAnonymousTemplate is not null)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.TemplateIdAmbiguous",
                Message: ErrorMessage.CopyTemplateToBranch_TemplateId_Ambiguous,
                Type: ErrorType.Validation));
        }

        if (sourceTemplate is not null)
        {
            return await CopyAuthorizedTemplateAsync(
                sourceTemplate,
                request.BranchId,
                currentApplicationUserId,
                cancellationToken);
        }

        if (sourceAnonymousTemplate is not null)
        {
            return await CopyAnonymousTemplateAsync(
                sourceAnonymousTemplate,
                request.BranchId,
                currentApplicationUserId,
                cancellationToken);
        }

        return Result<CopyTemplateToBranchResponse>.Fail(new Error(
            Code: "Templates.CopyToBranch.TemplateNotFound",
            Message: ErrorMessage.CopyTemplateToBranch_Template_NotFound,
            Type: ErrorType.NotFound));
    }

    private async Task<Result<CopyTemplateToBranchResponse>> CopyAuthorizedTemplateAsync(
        Template sourceTemplate,
        Guid targetBranchId,
        Guid currentApplicationUserId,
        CancellationToken cancellationToken)
    {
        var normalizedNameEn = sourceTemplate.NameEn.Trim();

        var alreadyExists = await _templateReadRepository.AnyAsync(
            x => x.BranchId == targetBranchId && x.NameEn == normalizedNameEn,
            cancellationToken);

        if (alreadyExists)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.TemplateAlreadyExists",
                Message: ErrorMessage.CopyTemplateToBranch_Template_AlreadyExists,
                Type: ErrorType.Validation));
        }

        var copiedTemplate = Template.Create(
            branchId: targetBranchId,
            nameEn: sourceTemplate.NameEn,
            nameAr: sourceTemplate.NameAr,
            description: sourceTemplate.Description,
            activeFrom: sourceTemplate.ActiveFrom,
            expireTo: sourceTemplate.ExpireTo,
            createdByApplicationUserId: currentApplicationUserId);

        ApplyTemplateStatus(copiedTemplate, sourceTemplate.Status);

        foreach (var sourceInput in sourceTemplate.CustomInputs.OrderBy(x => x.Order))
        {
            if (!sourceInput.IsActive)
            {
                continue;
            }

            copiedTemplate.AddCustomInput(TemplateCustomInput.Create(
                templateId: copiedTemplate.Id,
                name: sourceInput.Name,
                labelEn: sourceInput.LabelEn,
                labelAr: sourceInput.LabelAr,
                type: sourceInput.Type,
                isRequired: sourceInput.IsRequired,
                minLength: sourceInput.MinLength,
                maxLength: sourceInput.MaxLength,
                minValue: sourceInput.MinValue,
                maxValue: sourceInput.MaxValue,
                startWith: sourceInput.StartWith,
                order: sourceInput.Order,
                createdByApplicationUserId: currentApplicationUserId));
        }

        var copiedQuestions = new List<TemplateQuestion>();
        var templateQuestionIdMap = new Dictionary<Guid, Guid>();

        foreach (var sourceQuestion in sourceTemplate.TemplateQuestions.OrderBy(x => x.Order))
        {
            var copiedQuestion = TemplateQuestion.Create(
                templateId: copiedTemplate.Id,
                questionId: sourceQuestion.QuestionId,
                order: sourceQuestion.Order,
                createdByApplicationUserId: currentApplicationUserId);

            copiedQuestions.Add(copiedQuestion);
            templateQuestionIdMap[sourceQuestion.Id] = copiedQuestion.Id;
        }

        var sourceConditions = await _templateConditionReadRepository.ListAsync(
            new GetTemplateQuestionConditionsForCopyToBranchSpec(sourceTemplate.Id),
            cancellationToken);

        var copiedConditions = sourceConditions
            .Where(x =>
                templateQuestionIdMap.ContainsKey(x.ParentTemplateQuestionId) &&
                templateQuestionIdMap.ContainsKey(x.ChildTemplateQuestionId))
            .Select(x => CopyCondition(
                copiedTemplate.Id,
                x,
                templateQuestionIdMap,
                currentApplicationUserId))
            .ToList();

        await _templateWriteRepository.AddAsync(copiedTemplate, cancellationToken);

        if (copiedQuestions.Count > 0)
        {
            await _templateQuestionWriteRepository.AddRangeAsync(
                copiedQuestions,
                cancellationToken);
        }

        if (copiedConditions.Count > 0)
        {
            await _templateConditionWriteRepository.AddRangeAsync(
                copiedConditions,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CopyTemplateToBranchResponse>.Ok(new CopyTemplateToBranchResponse
        {
            SourceTemplateId = sourceTemplate.Id,
            TemplateId = copiedTemplate.Id,
            BranchId = copiedTemplate.BranchId,
            TemplateKind = TemplateCatalogKind.Authorized,
            NameEn = copiedTemplate.NameEn,
            NameAr = copiedTemplate.NameAr,
            Description = copiedTemplate.Description,
            ActiveFrom = copiedTemplate.ActiveFrom,
            ExpireTo = copiedTemplate.ExpireTo,
            Status = copiedTemplate.Status.ToString(),
            IsActive = copiedTemplate.IsActive,
            QuestionsCount = copiedQuestions.Count,
            ConditionsCount = copiedConditions.Count,
            CustomInputsCount = copiedTemplate.CustomInputs.Count,
            PublicUrl = null,
            QrCode = null
        });
    }

    private async Task<Result<CopyTemplateToBranchResponse>> CopyAnonymousTemplateAsync(
        AnonymousTemplate sourceTemplate,
        Guid targetBranchId,
        Guid currentApplicationUserId,
        CancellationToken cancellationToken)
    {
        var normalizedNameEn = sourceTemplate.NameEn.Trim();

        var alreadyExists = await _anonymousTemplateReadRepository.AnyAsync(
            x => x.Scope == AnonymousTemplateScope.Branch &&
                 x.BranchId == targetBranchId &&
                 x.NameEn == normalizedNameEn,
            cancellationToken);

        if (alreadyExists)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.TemplateAlreadyExists",
                Message: ErrorMessage.CopyTemplateToBranch_Template_AlreadyExists,
                Type: ErrorType.Validation));
        }

        var copiedTemplate = AnonymousTemplate.CreateBranchTemplate(
            branchId: targetBranchId,
            nameEn: sourceTemplate.NameEn,
            nameAr: sourceTemplate.NameAr,
            description: sourceTemplate.Description,
            activeFrom: sourceTemplate.ActiveFrom,
            expireTo: sourceTemplate.ExpireTo,
            createdByApplicationUserId: currentApplicationUserId);

        ApplyAnonymousTemplateStatus(copiedTemplate, sourceTemplate.Status);

        foreach (var sourceInput in sourceTemplate.CustomInputs.OrderBy(x => x.Order))
        {
            if (!sourceInput.IsActive)
            {
                continue;
            }

            copiedTemplate.AddCustomInput(AnonymousTemplateCustomInput.Create(
                anonymousTemplateId: copiedTemplate.Id,
                name: sourceInput.Name,
                labelEn: sourceInput.LabelEn,
                labelAr: sourceInput.LabelAr,
                type: sourceInput.Type,
                isRequired: sourceInput.IsRequired,
                minLength: sourceInput.MinLength,
                maxLength: sourceInput.MaxLength,
                minValue: sourceInput.MinValue,
                maxValue: sourceInput.MaxValue,
                startWith: sourceInput.StartWith,
                order: sourceInput.Order,
                createdByApplicationUserId: currentApplicationUserId));
        }

        var publicUrl = _publicSurveyUrlBuilder.BuildAnonymousTemplateUrl(
            copiedTemplate.Id);

        var qrCode = _qrCodeGenerator.GenerateBase64Png(publicUrl);

        copiedTemplate.SetPublicAccess(publicUrl, qrCode);

        var copiedQuestions = new List<AnonymousTemplateQuestion>();
        var templateQuestionIdMap = new Dictionary<Guid, Guid>();

        foreach (var sourceQuestion in sourceTemplate.Questions.OrderBy(x => x.Order))
        {
            var copiedQuestion = AnonymousTemplateQuestion.Create(
                anonymousTemplateId: copiedTemplate.Id,
                questionId: sourceQuestion.QuestionId,
                order: sourceQuestion.Order,
                createdByApplicationUserId: currentApplicationUserId);

            copiedQuestions.Add(copiedQuestion);
            templateQuestionIdMap[sourceQuestion.Id] = copiedQuestion.Id;
        }

        var sourceConditions = await _anonymousTemplateConditionReadRepository.ListAsync(
            new GetAnonymousTemplateQuestionConditionsForCopyToBranchSpec(sourceTemplate.Id),
            cancellationToken);

        var copiedConditions = sourceConditions
            .Where(x =>
                templateQuestionIdMap.ContainsKey(x.ParentAnonymousTemplateQuestionId) &&
                templateQuestionIdMap.ContainsKey(x.ChildAnonymousTemplateQuestionId))
            .Select(x => CopyAnonymousCondition(
                copiedTemplate.Id,
                x,
                templateQuestionIdMap,
                currentApplicationUserId))
            .ToList();

        await _anonymousTemplateWriteRepository.AddAsync(
            copiedTemplate,
            cancellationToken);

        if (copiedQuestions.Count > 0)
        {
            await _anonymousTemplateQuestionWriteRepository.AddRangeAsync(
                copiedQuestions,
                cancellationToken);
        }

        if (copiedConditions.Count > 0)
        {
            await _anonymousTemplateConditionWriteRepository.AddRangeAsync(
                copiedConditions,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CopyTemplateToBranchResponse>.Ok(new CopyTemplateToBranchResponse
        {
            SourceTemplateId = sourceTemplate.Id,
            TemplateId = copiedTemplate.Id,
            BranchId = copiedTemplate.BranchId!.Value,
            TemplateKind = TemplateCatalogKind.Anonymous,
            NameEn = copiedTemplate.NameEn,
            NameAr = copiedTemplate.NameAr,
            Description = copiedTemplate.Description,
            ActiveFrom = copiedTemplate.ActiveFrom,
            ExpireTo = copiedTemplate.ExpireTo,
            Status = copiedTemplate.Status.ToString(),
            IsActive = copiedTemplate.IsActive,
            QuestionsCount = copiedQuestions.Count,
            ConditionsCount = copiedConditions.Count,
            CustomInputsCount = copiedTemplate.CustomInputs.Count,
            PublicUrl = copiedTemplate.PublicUrl,
            QrCode = copiedTemplate.QrCode
        });
    }

    private static void ApplyTemplateStatus(
        Template template,
        TemplateStatus status)
    {
        if (status == TemplateStatus.Active)
        {
            template.Activate();
            return;
        }

        if (status == TemplateStatus.Inactive)
        {
            template.Deactivate();
        }
    }

    private static void ApplyAnonymousTemplateStatus(
        AnonymousTemplate template,
        TemplateStatus status)
    {
        if (status == TemplateStatus.Active)
        {
            template.Activate();
            return;
        }

        if (status == TemplateStatus.Inactive)
        {
            template.Deactivate();
        }
    }

    private static TemplateQuestionCondition CopyCondition(
        Guid copiedTemplateId,
        TemplateQuestionCondition sourceCondition,
        IReadOnlyDictionary<Guid, Guid> templateQuestionIdMap,
        Guid currentApplicationUserId)
    {
        return sourceCondition.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                TemplateQuestionCondition.CreateForSingleChoice(
                    templateId: copiedTemplateId,
                    parentTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentTemplateQuestionId],
                    childTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildTemplateQuestionId],
                    selectedQuestionOptionId: sourceCondition.SelectedQuestionOptionId!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            QuestionConditionTriggerType.StarRatingValue =>
                TemplateQuestionCondition.CreateForStarRating(
                    templateId: copiedTemplateId,
                    parentTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentTemplateQuestionId],
                    childTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildTemplateQuestionId],
                    starRatingValue: sourceCondition.TriggerValue!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            QuestionConditionTriggerType.SmileValue =>
                TemplateQuestionCondition.CreateForSmiles(
                    templateId: copiedTemplateId,
                    parentTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentTemplateQuestionId],
                    childTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildTemplateQuestionId],
                    smileValue: sourceCondition.TriggerValue!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            _ => throw new InvalidOperationException(
                $"Unsupported template question condition trigger type '{sourceCondition.TriggerType}'.")
        };
    }

    private static AnonymousTemplateQuestionCondition CopyAnonymousCondition(
        Guid copiedTemplateId,
        AnonymousTemplateQuestionCondition sourceCondition,
        IReadOnlyDictionary<Guid, Guid> templateQuestionIdMap,
        Guid currentApplicationUserId)
    {
        return sourceCondition.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                AnonymousTemplateQuestionCondition.CreateForSingleChoice(
                    anonymousTemplateId: copiedTemplateId,
                    parentAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentAnonymousTemplateQuestionId],
                    childAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildAnonymousTemplateQuestionId],
                    selectedQuestionOptionId: sourceCondition.SelectedQuestionOptionId!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            QuestionConditionTriggerType.StarRatingValue =>
                AnonymousTemplateQuestionCondition.CreateForStarRating(
                    anonymousTemplateId: copiedTemplateId,
                    parentAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentAnonymousTemplateQuestionId],
                    childAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildAnonymousTemplateQuestionId],
                    starRatingValue: sourceCondition.TriggerValue!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            QuestionConditionTriggerType.SmileValue =>
                AnonymousTemplateQuestionCondition.CreateForSmiles(
                    anonymousTemplateId: copiedTemplateId,
                    parentAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentAnonymousTemplateQuestionId],
                    childAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildAnonymousTemplateQuestionId],
                    smileValue: sourceCondition.TriggerValue!.Value,
                    order: sourceCondition.Order,
                    createdByApplicationUserId: currentApplicationUserId),

            _ => throw new InvalidOperationException(
                $"Unsupported anonymous template question condition trigger type '{sourceCondition.TriggerType}'.")
        };
    }
}
