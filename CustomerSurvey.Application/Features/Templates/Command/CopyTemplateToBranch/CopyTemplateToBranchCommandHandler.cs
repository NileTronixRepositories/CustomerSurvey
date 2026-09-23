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
    private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
    private readonly IWriteReadRepository<Question> _questionReadRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly IWriteRepository<TemplateQuestion> _templateQuestionWriteRepository;
    private readonly IWriteRepository<TemplateQuestionCondition> _templateConditionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionWriteRepository;
    private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _anonymousTemplateConditionWriteRepository;
    private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
    private readonly IWriteRepository<Question> _questionWriteRepository;
    private readonly IWriteRepository<QuestionOption> _questionOptionWriteRepository;
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
        IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
        IWriteReadRepository<Question> questionReadRepository,
        IWriteReadRepository<QuestionOption> questionOptionReadRepository,
        IWriteRepository<Template> templateWriteRepository,
        IWriteRepository<TemplateQuestion> templateQuestionWriteRepository,
        IWriteRepository<TemplateQuestionCondition> templateConditionWriteRepository,
        IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
        IWriteRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionWriteRepository,
        IWriteRepository<AnonymousTemplateQuestionCondition> anonymousTemplateConditionWriteRepository,
        IWriteRepository<QuestionGroup> questionGroupWriteRepository,
        IWriteRepository<Question> questionWriteRepository,
        IWriteRepository<QuestionOption> questionOptionWriteRepository,
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
        _questionGroupReadRepository = questionGroupReadRepository
            ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));
        _questionReadRepository = questionReadRepository
            ?? throw new ArgumentNullException(nameof(questionReadRepository));
        _questionOptionReadRepository = questionOptionReadRepository
            ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));
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
        _questionGroupWriteRepository = questionGroupWriteRepository
            ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));
        _questionWriteRepository = questionWriteRepository
            ?? throw new ArgumentNullException(nameof(questionWriteRepository));
        _questionOptionWriteRepository = questionOptionWriteRepository
            ?? throw new ArgumentNullException(nameof(questionOptionWriteRepository));
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

        var targetBranch = await _branchReadRepository.GetByPropertyAsync(
            x => x.Id == request.BranchId,
            cancellationToken);

        if (targetBranch is null)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.BranchNotFound",
                Message: ErrorMessage.CopyTemplateToBranch_Branch_NotFound,
                Type: ErrorType.NotFound));
        }

        if (!targetBranch.IsActive)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                Code: "Templates.CopyToBranch.BranchInactive",
                Message: "Target branch is inactive.",
                Type: ErrorType.Validation));
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
                targetBranch,
                currentApplicationUserId,
                cancellationToken);
        }

        if (sourceAnonymousTemplate is not null)
        {
            return await CopyAnonymousTemplateAsync(
                sourceAnonymousTemplate,
                targetBranch,
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
        Branch targetBranch,
        Guid currentApplicationUserId,
        CancellationToken cancellationToken)
    {
        var targetBranchId = targetBranch.Id;
        if (!sourceTemplate.IsActive)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.SourceInactive",
                "The source template is inactive.",
                ErrorType.Validation));
        }

        if (sourceTemplate.BranchId == targetBranchId)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.SameBranch",
                "Source and target branches must be different.",
                ErrorType.Validation));
        }

        var rootOriginId = sourceTemplate.OriginTemplateId ?? sourceTemplate.Id;
        var duplicateOrigin = await _templateReadRepository.AnyAsync(
            x => x.BranchId == targetBranchId && x.OriginTemplateId == rootOriginId,
            cancellationToken);

        if (duplicateOrigin)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.DuplicateOrigin",
                "The same logical template was already copied to the target branch.",
                ErrorType.Conflict));
        }

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
            createdByApplicationUserId: currentApplicationUserId,
            originTemplateId: rootOriginId);

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
        var optionIdMap = new Dictionary<Guid, Guid>();
        var newGroups = new List<QuestionGroup>();
        var newCatalogQuestions = new List<Question>();
        var newOptions = new List<QuestionOption>();
        var groupCache = new Dictionary<Guid, QuestionGroup>();
        var questionCache = new Dictionary<Guid, Question>();

        var groupConflict = await ValidateQuestionGroupLineageConflictsAsync(
            sourceTemplate.TemplateQuestions.Select(x => x.Question),
            targetBranchId,
            cancellationToken);

        if (groupConflict is not null)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(groupConflict);
        }

        foreach (var sourceQuestion in sourceTemplate.TemplateQuestions.OrderBy(x => x.Order))
        {
            var targetQuestion = await ResolveTargetQuestionAsync(
                sourceQuestion.Question,
                targetBranchId,
                currentApplicationUserId,
                groupCache,
                questionCache,
                optionIdMap,
                newGroups,
                newCatalogQuestions,
                newOptions,
                cancellationToken);

            var copiedQuestion = TemplateQuestion.Create(
                templateId: copiedTemplate.Id,
                questionId: targetQuestion.Id,
                order: sourceQuestion.Order,
                createdByApplicationUserId: currentApplicationUserId);

            copiedQuestions.Add(copiedQuestion);
            templateQuestionIdMap[sourceQuestion.Id] = copiedQuestion.Id;
        }

        var sourceConditions = await _templateConditionReadRepository.ListAsync(
            new GetTemplateQuestionConditionsForCopyToBranchSpec(sourceTemplate.Id),
            cancellationToken);

        if (sourceConditions.Any(x => x.SelectedQuestionOptionId.HasValue &&
                                      !optionIdMap.ContainsKey(x.SelectedQuestionOptionId.Value)))
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.QuestionOptionLineageConflict",
                "A required target question option lineage could not be resolved.",
                ErrorType.Conflict));
        }

        var copiedConditions = sourceConditions
            .Where(x =>
                templateQuestionIdMap.ContainsKey(x.ParentTemplateQuestionId) &&
                templateQuestionIdMap.ContainsKey(x.ChildTemplateQuestionId))
            .Select(x => CopyCondition(
                copiedTemplate.Id,
                x,
                templateQuestionIdMap,
                optionIdMap,
                currentApplicationUserId))
            .ToList();

        if (newGroups.Count > 0)
        {
            await _questionGroupWriteRepository.AddRangeAsync(newGroups, cancellationToken);
        }
        if (newCatalogQuestions.Count > 0)
        {
            await _questionWriteRepository.AddRangeAsync(newCatalogQuestions, cancellationToken);
        }
        if (newOptions.Count > 0)
        {
            await _questionOptionWriteRepository.AddRangeAsync(newOptions, cancellationToken);
        }

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
            BranchNameEn = targetBranch.NameEn,
            BranchNameAr = targetBranch.NameAr,
            TemplateKind = TemplateCatalogKind.Authorized,
            NameEn = copiedTemplate.NameEn,
            NameAr = copiedTemplate.NameAr,
            Description = copiedTemplate.Description,
            ActiveFrom = copiedTemplate.ActiveFrom,
            ExpireTo = copiedTemplate.ExpireTo,
            IsActive = copiedTemplate.IsActive,
            LogoPath = null,
            QuestionsCount = copiedQuestions.Count,
            ConditionsCount = copiedConditions.Count,
            CustomInputsCount = copiedTemplate.CustomInputs.Count,
            PublicUrl = null,
            QrCode = null
        });
    }

    private async Task<Result<CopyTemplateToBranchResponse>> CopyAnonymousTemplateAsync(
        AnonymousTemplate sourceTemplate,
        Branch targetBranch,
        Guid currentApplicationUserId,
        CancellationToken cancellationToken)
    {
        var targetBranchId = targetBranch.Id;
        if (!sourceTemplate.IsBranchScoped)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.SourceMustBeBranchScoped",
                "The source anonymous template must be branch scoped.",
                ErrorType.Validation));
        }

        if (!sourceTemplate.IsActive)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.SourceInactive",
                "The source anonymous template is inactive.",
                ErrorType.Validation));
        }

        if (sourceTemplate.SourceGlobalAnonymousTemplateId.HasValue)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.ManagedGlobalCopyForbidden",
                "Managed global branch copies must be distributed from their global source.",
                ErrorType.Validation));
        }

        if (sourceTemplate.BranchId == targetBranchId)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.SameBranch",
                "Source and target branches must be different.",
                ErrorType.Validation));
        }

        var rootOriginId = sourceTemplate.OriginAnonymousTemplateId ?? sourceTemplate.Id;
        var duplicateOrigin = await _anonymousTemplateReadRepository.AnyAsync(
            x => x.BranchId == targetBranchId && x.OriginAnonymousTemplateId == rootOriginId,
            cancellationToken);

        if (duplicateOrigin)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.DuplicateOrigin",
                "The same logical anonymous template was already copied to the target branch.",
                ErrorType.Conflict));
        }

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
            createdByApplicationUserId: currentApplicationUserId,
            originAnonymousTemplateId: rootOriginId);

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
        var optionIdMap = new Dictionary<Guid, Guid>();
        var newGroups = new List<QuestionGroup>();
        var newCatalogQuestions = new List<Question>();
        var newOptions = new List<QuestionOption>();
        var groupCache = new Dictionary<Guid, QuestionGroup>();
        var questionCache = new Dictionary<Guid, Question>();

        var groupConflict = await ValidateQuestionGroupLineageConflictsAsync(
            sourceTemplate.Questions.Select(x => x.Question),
            targetBranchId,
            cancellationToken);

        if (groupConflict is not null)
        {
            return Result<CopyTemplateToBranchResponse>.Fail(groupConflict);
        }

        foreach (var sourceQuestion in sourceTemplate.Questions.OrderBy(x => x.Order))
        {
            var targetQuestion = await ResolveTargetQuestionAsync(
                sourceQuestion.Question,
                targetBranchId,
                currentApplicationUserId,
                groupCache,
                questionCache,
                optionIdMap,
                newGroups,
                newCatalogQuestions,
                newOptions,
                cancellationToken);

            var copiedQuestion = AnonymousTemplateQuestion.Create(
                anonymousTemplateId: copiedTemplate.Id,
                questionId: targetQuestion.Id,
                order: sourceQuestion.Order,
                createdByApplicationUserId: currentApplicationUserId);

            copiedQuestions.Add(copiedQuestion);
            templateQuestionIdMap[sourceQuestion.Id] = copiedQuestion.Id;
        }

        var sourceConditions = await _anonymousTemplateConditionReadRepository.ListAsync(
            new GetAnonymousTemplateQuestionConditionsForCopyToBranchSpec(sourceTemplate.Id),
            cancellationToken);

        if (sourceConditions.Any(x => x.SelectedQuestionOptionId.HasValue &&
                                      !optionIdMap.ContainsKey(x.SelectedQuestionOptionId.Value)))
        {
            return Result<CopyTemplateToBranchResponse>.Fail(new Error(
                "Templates.CopyToBranch.QuestionOptionLineageConflict",
                "A required target question option lineage could not be resolved.",
                ErrorType.Conflict));
        }

        var copiedConditions = sourceConditions
            .Where(x =>
                templateQuestionIdMap.ContainsKey(x.ParentAnonymousTemplateQuestionId) &&
                templateQuestionIdMap.ContainsKey(x.ChildAnonymousTemplateQuestionId))
            .Select(x => CopyAnonymousCondition(
                copiedTemplate.Id,
                x,
                templateQuestionIdMap,
                optionIdMap,
                currentApplicationUserId))
            .ToList();

        if (newGroups.Count > 0)
        {
            await _questionGroupWriteRepository.AddRangeAsync(newGroups, cancellationToken);
        }
        if (newCatalogQuestions.Count > 0)
        {
            await _questionWriteRepository.AddRangeAsync(newCatalogQuestions, cancellationToken);
        }
        if (newOptions.Count > 0)
        {
            await _questionOptionWriteRepository.AddRangeAsync(newOptions, cancellationToken);
        }

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
            BranchNameEn = targetBranch.NameEn,
            BranchNameAr = targetBranch.NameAr,
            TemplateKind = TemplateCatalogKind.Anonymous,
            NameEn = copiedTemplate.NameEn,
            NameAr = copiedTemplate.NameAr,
            Description = copiedTemplate.Description,
            ActiveFrom = copiedTemplate.ActiveFrom,
            ExpireTo = copiedTemplate.ExpireTo,
            IsActive = copiedTemplate.IsActive,
            LogoPath = null,
            QuestionsCount = copiedQuestions.Count,
            ConditionsCount = copiedConditions.Count,
            CustomInputsCount = copiedTemplate.CustomInputs.Count,
            PublicUrl = copiedTemplate.PublicUrl,
            QrCode = copiedTemplate.QrCode
        });
    }

    private async Task<Error?> ValidateQuestionGroupLineageConflictsAsync(
        IEnumerable<Question> sourceQuestions,
        Guid targetBranchId,
        CancellationToken cancellationToken)
    {
        var groups = sourceQuestions
            .Where(x => x.Scope == QuestionScope.Branch)
            .Select(x => x.Group)
            .DistinctBy(x => x.Id)
            .ToArray();

        foreach (var group in groups)
        {
            var rootOriginId = group.OriginQuestionGroupId ?? group.Id;
            var existingOrigin = await _questionGroupReadRepository.AnyAsync(
                x => x.BranchId == targetBranchId && x.OriginQuestionGroupId == rootOriginId,
                cancellationToken);

            if (existingOrigin)
            {
                continue;
            }

            var nameConflict = await _questionGroupReadRepository.AnyAsync(
                x => x.BranchId == targetBranchId && x.NameEn == group.NameEn,
                cancellationToken);

            if (nameConflict)
            {
                return new Error(
                    "Templates.CopyToBranch.QuestionGroupLineageConflict",
                    $"Question group '{group.NameEn}' conflicts with an unrelated target branch group.",
                    ErrorType.Conflict);
            }
        }

        return null;
    }

    private async Task<Question> ResolveTargetQuestionAsync(
        Question sourceQuestion,
        Guid targetBranchId,
        Guid currentApplicationUserId,
        IDictionary<Guid, QuestionGroup> groupCache,
        IDictionary<Guid, Question> questionCache,
        IDictionary<Guid, Guid> optionIdMap,
        ICollection<QuestionGroup> newGroups,
        ICollection<Question> newQuestions,
        ICollection<QuestionOption> newOptions,
        CancellationToken cancellationToken)
    {
        if (sourceQuestion.Scope == QuestionScope.Global)
        {
            foreach (var option in sourceQuestion.Options)
            {
                optionIdMap[option.Id] = option.Id;
            }

            return sourceQuestion;
        }

        var rootGroupId = sourceQuestion.Group.OriginQuestionGroupId ?? sourceQuestion.Group.Id;
        if (!groupCache.TryGetValue(rootGroupId, out var targetGroup))
        {
            targetGroup = await _questionGroupReadRepository.GetByPropertyAsync(
                x => x.BranchId == targetBranchId && x.OriginQuestionGroupId == rootGroupId,
                cancellationToken);

            if (targetGroup is null)
            {
                targetGroup = QuestionGroup.Create(
                    targetBranchId,
                    sourceQuestion.Group.NameEn,
                    sourceQuestion.Group.NameAr,
                    currentApplicationUserId,
                    rootGroupId);
                newGroups.Add(targetGroup);
            }

            groupCache[rootGroupId] = targetGroup;
        }

        var rootQuestionId = sourceQuestion.OriginQuestionId ?? sourceQuestion.Id;
        if (!questionCache.TryGetValue(rootQuestionId, out var targetQuestion))
        {
            targetQuestion = await _questionReadRepository.GetByPropertyAsync(
                x => x.BranchId == targetBranchId && x.OriginQuestionId == rootQuestionId,
                cancellationToken);

            if (targetQuestion is null)
            {
                targetQuestion = Question.Create(
                    targetBranchId,
                    targetGroup.Id,
                    sourceQuestion.TextEn,
                    sourceQuestion.TextAr,
                    sourceQuestion.Type,
                    currentApplicationUserId,
                    rootQuestionId);
                newQuestions.Add(targetQuestion);

                foreach (var sourceOption in sourceQuestion.Options.Where(x => x.IsActive).OrderBy(x => x.Order))
                {
                    var rootOptionId = sourceOption.OriginQuestionOptionId ?? sourceOption.Id;
                    var targetOption = QuestionOption.Create(
                        targetQuestion.Id,
                        sourceOption.TextEn,
                        sourceOption.TextAr,
                        sourceOption.Order,
                        sourceOption.Value,
                        currentApplicationUserId,
                        rootOptionId);
                    newOptions.Add(targetOption);
                    optionIdMap[sourceOption.Id] = targetOption.Id;
                }
            }
            else
            {
                var targetOptions = await _questionOptionReadRepository.ListAsync(
                    new GetTargetQuestionOptionsForCopyToBranchSpec(targetQuestion.Id),
                    cancellationToken);

                foreach (var sourceOption in sourceQuestion.Options.Where(x => x.IsActive))
                {
                    var rootOptionId = sourceOption.OriginQuestionOptionId ?? sourceOption.Id;
                    var targetOption = targetOptions.FirstOrDefault(
                        x => x.OriginQuestionOptionId == rootOptionId);

                    if (targetOption is not null)
                    {
                        optionIdMap[sourceOption.Id] = targetOption.Id;
                    }
                }
            }

            questionCache[rootQuestionId] = targetQuestion;
        }
        else
        {
            foreach (var sourceOption in sourceQuestion.Options.Where(x => x.IsActive))
            {
                var rootOptionId = sourceOption.OriginQuestionOptionId ?? sourceOption.Id;
                var targetOption = newOptions.FirstOrDefault(
                    x => x.QuestionId == targetQuestion.Id &&
                         x.OriginQuestionOptionId == rootOptionId);

                if (targetOption is not null)
                {
                    optionIdMap[sourceOption.Id] = targetOption.Id;
                }
            }
        }

        return targetQuestion;
    }

    private static TemplateQuestionCondition CopyCondition(
        Guid copiedTemplateId,
        TemplateQuestionCondition sourceCondition,
        IReadOnlyDictionary<Guid, Guid> templateQuestionIdMap,
        IReadOnlyDictionary<Guid, Guid> optionIdMap,
        Guid currentApplicationUserId)
    {
        return sourceCondition.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                TemplateQuestionCondition.CreateForSingleChoice(
                    templateId: copiedTemplateId,
                    parentTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentTemplateQuestionId],
                    childTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildTemplateQuestionId],
                    selectedQuestionOptionId: optionIdMap[sourceCondition.SelectedQuestionOptionId!.Value],
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
        IReadOnlyDictionary<Guid, Guid> optionIdMap,
        Guid currentApplicationUserId)
    {
        return sourceCondition.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                AnonymousTemplateQuestionCondition.CreateForSingleChoice(
                    anonymousTemplateId: copiedTemplateId,
                    parentAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ParentAnonymousTemplateQuestionId],
                    childAnonymousTemplateQuestionId: templateQuestionIdMap[sourceCondition.ChildAnonymousTemplateQuestionId],
                    selectedQuestionOptionId: optionIdMap[sourceCondition.SelectedQuestionOptionId!.Value],
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
