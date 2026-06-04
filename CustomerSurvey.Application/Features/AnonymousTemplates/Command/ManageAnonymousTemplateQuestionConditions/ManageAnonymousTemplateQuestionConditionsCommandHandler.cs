using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class ManageAnonymousTemplateQuestionConditionsCommandHandler
        : ICommandHandler<ManageAnonymousTemplateQuestionConditionsCommand, ManageAnonymousTemplateQuestionConditionsResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteRepository<AnonymousTemplateQuestionCondition> _conditionWriteRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ManageAnonymousTemplateQuestionConditionsCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
            IWriteRepository<AnonymousTemplateQuestionCondition> conditionWriteRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _conditionWriteRepository = conditionWriteRepository
                ?? throw new ArgumentNullException(nameof(conditionWriteRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<ManageAnonymousTemplateQuestionConditionsResponse>> Handle(
            ManageAnonymousTemplateQuestionConditionsCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ManageConditions.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var requestedConditions = request.Conditions
                ?? Array.Empty<ManageAnonymousTemplateQuestionConditionCommandItem>();

            var currentApplicationUserId = _currentUser.UserId.Value;

            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            Guid? currentBranchId = null;

            if (!isSuperAdmin)
            {
                var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                    cancellationToken);

                if (currentBranchScope.IsFailure)
                {
                    return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Fail(currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForManageConditionsSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ManageConditions.TemplateNotFound",
                    Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!anonymousTemplate.IsActive)
            {
                return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.ManageConditions.TemplateInactive",
                    Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var templateQuestions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetAnonymousTemplateQuestionsForManageConditionsSpec(anonymousTemplate.Id),
                cancellationToken);

            var questionsByTemplateQuestionId = templateQuestions
                .ToDictionary(x => x.AnonymousTemplateQuestionId);

            var validationResult = await ValidateConditionsAsync(
                requestedConditions,
                questionsByTemplateQuestionId,
                cancellationToken);

            if (validationResult.Error is not null)
            {
                return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Fail(
                    validationResult.Error);
            }

            var existingConditions = await _conditionReadRepository.ListAsync(
                new GetExistingAnonymousTemplateQuestionConditionsForManageSpec(anonymousTemplate.Id),
                cancellationToken);

            if (existingConditions.Count > 0)
            {
                _conditionWriteRepository.DeleteRange(existingConditions);
            }

            var newConditions = requestedConditions
                .OrderBy(x => x.Order)
                .Select(x => CreateCondition(
                    anonymousTemplateId: anonymousTemplate.Id,
                    requestItem: x,
                    currentApplicationUserId: currentApplicationUserId))
                .ToList();

            if (newConditions.Count > 0)
            {
                await _conditionWriteRepository.AddRangeAsync(
                    newConditions,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var responseConditions = newConditions
                .OrderBy(x => x.Order)
                .Select(x => new ManagedAnonymousTemplateQuestionConditionResponse
                {
                    ConditionId = x.Id,
                    ParentAnonymousTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                    ChildAnonymousTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
                    TriggerType = x.TriggerType,
                    TriggerTypeName = x.TriggerType.ToString(),
                    SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                    TriggerValue = x.TriggerValue,
                    Order = x.Order,
                    IsActive = x.IsActive
                })
                .ToArray();

            return Result<ManageAnonymousTemplateQuestionConditionsResponse>.Ok(
                new ManageAnonymousTemplateQuestionConditionsResponse
                {
                    AnonymousTemplateId = anonymousTemplate.Id,
                    BranchId = anonymousTemplate.BranchId,
                    Scope = anonymousTemplate.Scope,
                    ScopeName = anonymousTemplate.Scope.ToString(),
                    IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                    ConditionsCount = responseConditions.Length,
                    Conditions = responseConditions
                });
        }

        private async Task<ConditionsValidationResult> ValidateConditionsAsync(
            IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> conditions,
            IReadOnlyDictionary<Guid, AnonymousTemplateQuestionForConditionDto> questionsByTemplateQuestionId,
            CancellationToken cancellationToken)
        {
            if (conditions.Count == 0)
            {
                return ConditionsValidationResult.Ok();
            }

            foreach (var condition in conditions)
            {
                if (!questionsByTemplateQuestionId.TryGetValue(
                        condition.ParentAnonymousTemplateQuestionId,
                        out var parentQuestion))
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.ParentQuestionNotFound",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_ParentQuestion_NotFound,
                        Type: ErrorType.Validation));
                }

                if (!questionsByTemplateQuestionId.TryGetValue(
                        condition.ChildAnonymousTemplateQuestionId,
                        out var childQuestion))
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.ChildQuestionNotFound",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_ChildQuestion_NotFound,
                        Type: ErrorType.Validation));
                }

                if (!parentQuestion.QuestionIsActive || !childQuestion.QuestionIsActive)
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.QuestionInactive",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_Question_Inactive,
                        Type: ErrorType.Validation));
                }

                var parentTypeIsAllowed =
                    parentQuestion.QuestionType == QuestionType.SingleChoice ||
                    parentQuestion.QuestionType == QuestionType.StarRating ||
                    parentQuestion.QuestionType == QuestionType.Smiles;

                if (!parentTypeIsAllowed)
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.ParentQuestionTypeNotAllowed",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_ParentQuestionType_NotAllowed,
                        Type: ErrorType.Validation));
                }

                var triggerMatchesParentType = DoesTriggerTypeMatchParentQuestionType(
                    condition.TriggerType,
                    parentQuestion.QuestionType);

                if (!triggerMatchesParentType)
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.TriggerTypeDoesNotMatchParentQuestion",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_TriggerType_DoesNotMatchParentQuestion,
                        Type: ErrorType.Validation));
                }
            }

            var singleChoiceOptionIds = conditions
                .Where(x => x.TriggerType == QuestionConditionTriggerType.SingleChoiceOption)
                .Select(x => x.SelectedQuestionOptionId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToArray();

            if (singleChoiceOptionIds.Length > 0)
            {
                var optionValidationResult = await ValidateSingleChoiceOptionsAsync(
                    conditions,
                    questionsByTemplateQuestionId,
                    singleChoiceOptionIds,
                    cancellationToken);

                if (optionValidationResult.Error is not null)
                {
                    return optionValidationResult;
                }
            }

            if (HasCycle(conditions))
            {
                return ConditionsValidationResult.Fail(new Error(
                    Code: "AnonymousTemplates.ManageConditions.CircularFlowDetected",
                    Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_CircularFlow_Detected,
                    Type: ErrorType.Validation));
            }

            return ConditionsValidationResult.Ok();
        }

        private async Task<ConditionsValidationResult> ValidateSingleChoiceOptionsAsync(
            IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> conditions,
            IReadOnlyDictionary<Guid, AnonymousTemplateQuestionForConditionDto> questionsByTemplateQuestionId,
            IReadOnlyCollection<Guid> selectedOptionIds,
            CancellationToken cancellationToken)
        {
            var options = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForManageAnonymousTemplateConditionsSpec(selectedOptionIds),
                cancellationToken);

            if (options.Count != selectedOptionIds.Count)
            {
                return ConditionsValidationResult.Fail(new Error(
                    Code: "AnonymousTemplates.ManageConditions.SelectedOptionNotFound",
                    Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_SelectedOption_NotFound,
                    Type: ErrorType.Validation));
            }

            var optionsById = options.ToDictionary(x => x.OptionId);

            foreach (var condition in conditions.Where(x => x.TriggerType == QuestionConditionTriggerType.SingleChoiceOption))
            {
                var selectedOptionId = condition.SelectedQuestionOptionId!.Value;
                var selectedOption = optionsById[selectedOptionId];

                if (!selectedOption.IsActive)
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.SelectedOptionInactive",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_SelectedOption_Inactive,
                        Type: ErrorType.Validation));
                }

                var parentQuestion = questionsByTemplateQuestionId[
                    condition.ParentAnonymousTemplateQuestionId];

                if (selectedOption.QuestionId != parentQuestion.QuestionId)
                {
                    return ConditionsValidationResult.Fail(new Error(
                        Code: "AnonymousTemplates.ManageConditions.SelectedOptionDoesNotBelongToParentQuestion",
                        Message: ErrorMessage.ManageAnonymousTemplateQuestionConditions_SelectedOption_NotBelongToParentQuestion,
                        Type: ErrorType.Validation));
                }
            }

            return ConditionsValidationResult.Ok();
        }

        private static bool DoesTriggerTypeMatchParentQuestionType(
            QuestionConditionTriggerType triggerType,
            QuestionType parentQuestionType)
        {
            return parentQuestionType switch
            {
                QuestionType.SingleChoice =>
                    triggerType == QuestionConditionTriggerType.SingleChoiceOption,

                QuestionType.StarRating =>
                    triggerType == QuestionConditionTriggerType.StarRatingValue,

                QuestionType.Smiles =>
                    triggerType == QuestionConditionTriggerType.SmileValue,

                _ => false
            };
        }

        private static bool HasCycle(
            IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> conditions)
        {
            var graph = conditions
                .GroupBy(x => x.ParentAnonymousTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .Select(condition => condition.ChildAnonymousTemplateQuestionId)
                        .Distinct()
                        .ToArray());

            var visited = new HashSet<Guid>();
            var recursionStack = new HashSet<Guid>();

            foreach (var node in graph.Keys)
            {
                if (HasCycleFromNode(
                        node,
                        graph,
                        visited,
                        recursionStack))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCycleFromNode(
            Guid node,
            IReadOnlyDictionary<Guid, Guid[]> graph,
            HashSet<Guid> visited,
            HashSet<Guid> recursionStack)
        {
            if (recursionStack.Contains(node))
            {
                return true;
            }

            if (visited.Contains(node))
            {
                return false;
            }

            visited.Add(node);
            recursionStack.Add(node);

            if (graph.TryGetValue(node, out var children))
            {
                foreach (var child in children)
                {
                    if (HasCycleFromNode(
                            child,
                            graph,
                            visited,
                            recursionStack))
                    {
                        return true;
                    }
                }
            }

            recursionStack.Remove(node);

            return false;
        }

        private static AnonymousTemplateQuestionCondition CreateCondition(
            Guid anonymousTemplateId,
            ManageAnonymousTemplateQuestionConditionCommandItem requestItem,
            Guid currentApplicationUserId)
        {
            return requestItem.TriggerType switch
            {
                QuestionConditionTriggerType.SingleChoiceOption =>
                    AnonymousTemplateQuestionCondition.CreateForSingleChoice(
                        anonymousTemplateId: anonymousTemplateId,
                        parentAnonymousTemplateQuestionId: requestItem.ParentAnonymousTemplateQuestionId,
                        childAnonymousTemplateQuestionId: requestItem.ChildAnonymousTemplateQuestionId,
                        selectedQuestionOptionId: requestItem.SelectedQuestionOptionId!.Value,
                        order: requestItem.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                QuestionConditionTriggerType.StarRatingValue =>
                    AnonymousTemplateQuestionCondition.CreateForStarRating(
                        anonymousTemplateId: anonymousTemplateId,
                        parentAnonymousTemplateQuestionId: requestItem.ParentAnonymousTemplateQuestionId,
                        childAnonymousTemplateQuestionId: requestItem.ChildAnonymousTemplateQuestionId,
                        starRatingValue: requestItem.TriggerValue!.Value,
                        order: requestItem.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                QuestionConditionTriggerType.SmileValue =>
                    AnonymousTemplateQuestionCondition.CreateForSmiles(
                        anonymousTemplateId: anonymousTemplateId,
                        parentAnonymousTemplateQuestionId: requestItem.ParentAnonymousTemplateQuestionId,
                        childAnonymousTemplateQuestionId: requestItem.ChildAnonymousTemplateQuestionId,
                        smileValue: requestItem.TriggerValue!.Value,
                        order: requestItem.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                _ => throw new InvalidOperationException(
                    "Unsupported anonymous template question condition trigger type.")
            };
        }

        private sealed class ConditionsValidationResult
        {
            private ConditionsValidationResult(Error? error)
            {
                Error = error;
            }

            public Error? Error { get; }

            public static ConditionsValidationResult Ok()
            {
                return new ConditionsValidationResult(error: null);
            }

            public static ConditionsValidationResult Fail(Error error)
            {
                return new ConditionsValidationResult(error);
            }
        }
    }
}
