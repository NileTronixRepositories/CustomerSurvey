using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed class ManageTemplateQuestionConditionsCommandHandler
          : ICommandHandler<ManageTemplateQuestionConditionsCommand, ManageTemplateQuestionConditionsResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteRepository<TemplateQuestionCondition> _conditionWriteRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ManageTemplateQuestionConditionsCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
            IWriteRepository<TemplateQuestionCondition> conditionWriteRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));
            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));
            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));
            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));
            _conditionWriteRepository = conditionWriteRepository
                ?? throw new ArgumentNullException(nameof(conditionWriteRepository));
            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<ManageTemplateQuestionConditionsResponse>> Handle(
            ManageTemplateQuestionConditionsCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<ManageTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<ManageTemplateQuestionConditionsResponse>.Fail(
                    currentBranchScope.Errors);
            }

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForManageTemplateQuestionConditionsSpec(
                    templateId: request.TemplateId,
                    branchId: currentBranchScope.Value.BranchId),
                cancellationToken);

            if (template is null)
            {
                return Result<ManageTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.TemplateNotFound",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!template.IsActive || template.Status == TemplateStatus.Inactive)
            {
                return Result<ManageTemplateQuestionConditionsResponse>.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.TemplateInactive",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var templateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForManageTemplateQuestionConditionsSpec(template.TemplateId),
                cancellationToken);

            var templateQuestionsById = templateQuestions
                .ToDictionary(x => x.TemplateQuestionId);

            var validationResult = await ValidateConditionsAsync(
                request.Conditions,
                templateQuestionsById,
                cancellationToken);

            if (validationResult.IsFailure)
            {
                return Result<ManageTemplateQuestionConditionsResponse>.Fail(
                    validationResult.Errors);
            }

            var existingConditions = await _conditionReadRepository.ListAsync(
                new GetExistingTemplateQuestionConditionsSpec(template.TemplateId),
                cancellationToken);

            foreach (var existingCondition in existingConditions)
            {
                _conditionWriteRepository.Delete(existingCondition);
            }

            var newConditions = request.Conditions
                .OrderBy(x => x.Order)
                .Select(x => CreateCondition(
                    template.TemplateId,
                    x,
                    currentApplicationUserId))
                .ToArray();

            foreach (var condition in newConditions)
            {
                await _conditionWriteRepository.AddAsync(condition, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new ManageTemplateQuestionConditionsResponse
            {
                TemplateId = template.TemplateId,
                BranchId = template.BranchId,
                ConditionsCount = newConditions.Length,
                Conditions = newConditions
                    .OrderBy(x => x.Order)
                    .Select(x => new TemplateQuestionConditionResponse
                    {
                        ConditionId = x.Id,
                        ParentTemplateQuestionId = x.ParentTemplateQuestionId,
                        ChildTemplateQuestionId = x.ChildTemplateQuestionId,
                        TriggerType = (int)x.TriggerType,
                        TriggerTypeName = x.TriggerType.ToString(),
                        SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                        TriggerValue = x.TriggerValue,
                        Order = x.Order
                    })
                    .ToArray()
            };

            return Result<ManageTemplateQuestionConditionsResponse>.Ok(response);
        }

        private async Task<Result> ValidateConditionsAsync(
     IReadOnlyCollection<TemplateQuestionConditionCommandItem> conditions,
     IReadOnlyDictionary<Guid, TemplateQuestionForManageTemplateQuestionConditionsDto> templateQuestionsById,
     CancellationToken cancellationToken)
        {
            if (conditions.Count == 0)
            {
                return Result.Ok();
            }

            foreach (var condition in conditions)
            {
                if (condition.ParentTemplateQuestionId == condition.ChildTemplateQuestionId)
                {
                    return Result.Fail(new Error(
                        Code: "Templates.ManageQuestionConditions.ParentCannotEqualChild",
                        Message: ErrorMessage.ManageTemplateQuestionConditions_ParentCannotEqualChild,
                        Type: ErrorType.Validation));
                }

                if (!templateQuestionsById.TryGetValue(
                        condition.ParentTemplateQuestionId,
                        out var parentTemplateQuestion) ||
                    !templateQuestionsById.TryGetValue(
                        condition.ChildTemplateQuestionId,
                        out var childTemplateQuestion))
                {
                    return Result.Fail(new Error(
                        Code: "Templates.ManageQuestionConditions.TemplateQuestionNotFound",
                        Message: ErrorMessage.ManageTemplateQuestionConditions_TemplateQuestion_NotFound,
                        Type: ErrorType.Validation));
                }

                var parentUsableResult = ValidateTemplateQuestionIsUsable(
                    templateQuestion: parentTemplateQuestion,
                    inactiveQuestionCode: "Templates.ManageQuestionConditions.ParentQuestionInactive",
                    inactiveQuestionMessage: ErrorMessage.ManageTemplateQuestionConditions_ParentQuestion_Inactive,
                    inactiveGroupCode: "Templates.ManageQuestionConditions.ParentQuestionGroupInactive",
                    inactiveGroupMessage: ErrorMessage.ManageTemplateQuestionConditions_ParentQuestionGroup_Inactive);

                if (parentUsableResult.IsFailure)
                {
                    return parentUsableResult;
                }

                var childUsableResult = ValidateTemplateQuestionIsUsable(
                    templateQuestion: childTemplateQuestion,
                    inactiveQuestionCode: "Templates.ManageQuestionConditions.ChildQuestionInactive",
                    inactiveQuestionMessage: ErrorMessage.ManageTemplateQuestionConditions_ChildQuestion_Inactive,
                    inactiveGroupCode: "Templates.ManageQuestionConditions.ChildQuestionGroupInactive",
                    inactiveGroupMessage: ErrorMessage.ManageTemplateQuestionConditions_ChildQuestionGroup_Inactive);

                if (childUsableResult.IsFailure)
                {
                    return childUsableResult;
                }
            }

            var selectedOptionIds = conditions
                .Where(x => x.SelectedQuestionOptionId.HasValue)
                .Select(x => x.SelectedQuestionOptionId!.Value)
                .Distinct()
                .ToArray();

            var optionsById = new Dictionary<Guid, QuestionOptionForManageTemplateQuestionConditionsDto>();

            if (selectedOptionIds.Length > 0)
            {
                var options = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsForManageTemplateQuestionConditionsSpec(selectedOptionIds),
                    cancellationToken);

                optionsById = options.ToDictionary(x => x.OptionId);
            }

            foreach (var condition in conditions)
            {
                var parentTemplateQuestion = templateQuestionsById[condition.ParentTemplateQuestionId];

                var result = ValidateSingleCondition(
                    condition,
                    parentTemplateQuestion,
                    optionsById);

                if (result.IsFailure)
                {
                    return result;
                }
            }

            if (HasCycle(conditions))
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.CycleDetected",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_CycleDetected,
                    Type: ErrorType.Validation));
            }

            return Result.Ok();
        }

        private static Result ValidateTemplateQuestionIsUsable(
    TemplateQuestionForManageTemplateQuestionConditionsDto templateQuestion,
    string inactiveQuestionCode,
    string inactiveQuestionMessage,
    string inactiveGroupCode,
    string inactiveGroupMessage)
        {
            if (!templateQuestion.QuestionIsActive)
            {
                return Result.Fail(new Error(
                    Code: inactiveQuestionCode,
                    Message: inactiveQuestionMessage,
                    Type: ErrorType.Validation));
            }

            if (!templateQuestion.QuestionGroupIsActive)
            {
                return Result.Fail(new Error(
                    Code: inactiveGroupCode,
                    Message: inactiveGroupMessage,
                    Type: ErrorType.Validation));
            }

            return Result.Ok();
        }

        private static Result ValidateSingleCondition(
            TemplateQuestionConditionCommandItem condition,
            TemplateQuestionForManageTemplateQuestionConditionsDto parentTemplateQuestion,
            IReadOnlyDictionary<Guid, QuestionOptionForManageTemplateQuestionConditionsDto> optionsById)
        {
            return parentTemplateQuestion.QuestionType switch
            {
                QuestionType.SingleChoice => ValidateSingleChoiceCondition(
                    condition,
                    parentTemplateQuestion,
                    optionsById),

                QuestionType.StarRating => ValidateStarRatingCondition(condition),

                QuestionType.Smiles => ValidateSmilesCondition(condition),

                QuestionType.Image => Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.ImageParentTypeNotAllowed",
                    Message: ErrorMessage.Question_Image_ParentCondition_NotAllowed,
                    Type: ErrorType.Validation)),

                QuestionType.Voice or QuestionType.Complain => Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.ParentTypeNotAllowed",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_ParentType_NotAllowed,
                    Type: ErrorType.Validation)),

                _ => Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.ParentTypeNotAllowed",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_ParentType_NotAllowed,
                    Type: ErrorType.Validation))
            };
        }

        private static Result ValidateSingleChoiceCondition(
            TemplateQuestionConditionCommandItem condition,
            TemplateQuestionForManageTemplateQuestionConditionsDto parentTemplateQuestion,
            IReadOnlyDictionary<Guid, QuestionOptionForManageTemplateQuestionConditionsDto> optionsById)
        {
            if (condition.TriggerType != QuestionConditionTriggerType.SingleChoiceOption)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.TriggerTypeMismatch",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_TriggerType_Mismatch,
                    Type: ErrorType.Validation));
            }

            if (!condition.SelectedQuestionOptionId.HasValue || condition.TriggerValue.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.SingleChoiceShapeInvalid",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_SingleChoiceShape_Invalid,
                    Type: ErrorType.Validation));
            }

            if (!optionsById.TryGetValue(condition.SelectedQuestionOptionId.Value, out var option))
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.SelectedOptionInvalid",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_SelectedOption_Invalid,
                    Type: ErrorType.Validation));
            }

            if (option.QuestionId != parentTemplateQuestion.QuestionId)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.SelectedOptionDoesNotBelongToParent",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_SelectedOption_NotBelongToParent,
                    Type: ErrorType.Validation));
            }

            return Result.Ok();
        }

        private static Result ValidateStarRatingCondition(
            TemplateQuestionConditionCommandItem condition)
        {
            if (condition.TriggerType != QuestionConditionTriggerType.StarRatingValue)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.TriggerTypeMismatch",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_TriggerType_Mismatch,
                    Type: ErrorType.Validation));
            }

            if (condition.SelectedQuestionOptionId.HasValue ||
                !condition.TriggerValue.HasValue ||
                condition.TriggerValue.Value is < 1 or > 5)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.StarRatingShapeInvalid",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_StarRatingShape_Invalid,
                    Type: ErrorType.Validation));
            }

            return Result.Ok();
        }

        private static Result ValidateSmilesCondition(
            TemplateQuestionConditionCommandItem condition)
        {
            if (condition.TriggerType != QuestionConditionTriggerType.SmileValue)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.TriggerTypeMismatch",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_TriggerType_Mismatch,
                    Type: ErrorType.Validation));
            }

            if (condition.SelectedQuestionOptionId.HasValue ||
                !condition.TriggerValue.HasValue ||
                condition.TriggerValue.Value is < 1 or > 5)
            {
                return Result.Fail(new Error(
                    Code: "Templates.ManageQuestionConditions.SmilesShapeInvalid",
                    Message: ErrorMessage.ManageTemplateQuestionConditions_SmilesShape_Invalid,
                    Type: ErrorType.Validation));
            }

            return Result.Ok();
        }

        private static bool HasCycle(
            IReadOnlyCollection<TemplateQuestionConditionCommandItem> conditions)
        {
            var graph = conditions
                .GroupBy(x => x.ParentTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(item => item.ChildTemplateQuestionId).Distinct().ToArray());

            var visited = new HashSet<Guid>();
            var visiting = new HashSet<Guid>();

            foreach (var node in graph.Keys)
            {
                if (Visit(node, graph, visited, visiting))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Visit(
            Guid node,
            IReadOnlyDictionary<Guid, Guid[]> graph,
            HashSet<Guid> visited,
            HashSet<Guid> visiting)
        {
            if (visited.Contains(node))
            {
                return false;
            }

            if (!visiting.Add(node))
            {
                return true;
            }

            if (graph.TryGetValue(node, out var children))
            {
                foreach (var child in children)
                {
                    if (Visit(child, graph, visited, visiting))
                    {
                        return true;
                    }
                }
            }

            visiting.Remove(node);
            visited.Add(node);

            return false;
        }

        private static TemplateQuestionCondition CreateCondition(
            Guid templateId,
            TemplateQuestionConditionCommandItem item,
            Guid currentApplicationUserId)
        {
            return item.TriggerType switch
            {
                QuestionConditionTriggerType.SingleChoiceOption =>
                    TemplateQuestionCondition.CreateForSingleChoice(
                        templateId: templateId,
                        parentTemplateQuestionId: item.ParentTemplateQuestionId,
                        childTemplateQuestionId: item.ChildTemplateQuestionId,
                        selectedQuestionOptionId: item.SelectedQuestionOptionId!.Value,
                        order: item.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                QuestionConditionTriggerType.StarRatingValue =>
                    TemplateQuestionCondition.CreateForStarRating(
                        templateId: templateId,
                        parentTemplateQuestionId: item.ParentTemplateQuestionId,
                        childTemplateQuestionId: item.ChildTemplateQuestionId,
                        starRatingValue: item.TriggerValue!.Value,
                        order: item.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                QuestionConditionTriggerType.SmileValue =>
                    TemplateQuestionCondition.CreateForSmiles(
                        templateId: templateId,
                        parentTemplateQuestionId: item.ParentTemplateQuestionId,
                        childTemplateQuestionId: item.ChildTemplateQuestionId,
                        smileValue: item.TriggerValue!.Value,
                        order: item.Order,
                        createdByApplicationUserId: currentApplicationUserId),

                _ => throw new InvalidOperationException(
                    $"Unsupported trigger type '{item.TriggerType}'.")
            };
        }

    }
}
