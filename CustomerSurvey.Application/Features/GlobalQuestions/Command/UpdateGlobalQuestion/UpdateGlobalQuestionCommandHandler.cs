using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class UpdateGlobalQuestionCommandHandler
         : ICommandHandler<UpdateGlobalQuestionCommand, UpdateGlobalQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteRepository<QuestionOption> _questionOptionWriteRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _templateQuestionConditionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateGlobalQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteRepository<QuestionOption> questionOptionWriteRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> templateQuestionConditionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _questionOptionWriteRepository = questionOptionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionOptionWriteRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _templateQuestionConditionReadRepository = templateQuestionConditionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionConditionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<UpdateGlobalQuestionResponse>> Handle(
            UpdateGlobalQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForUpdateGlobalQuestionSpec(currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Update.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.UpdateGlobalQuestion_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionForUpdateSpec(request.QuestionId),
                cancellationToken);

            if (question is null)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Update.QuestionNotFound",
                    Message: ErrorMessage.UpdateGlobalQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionGroupForUpdateGlobalQuestionSpec(request.GroupId),
                cancellationToken);

            if (group is null)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Update.QuestionGroupNotFound",
                    Message: ErrorMessage.UpdateGlobalQuestion_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            var isAssignedToTemplate = await _templateQuestionReadRepository.AnyAsync(
                x => x.QuestionId == question.Id,
                cancellationToken);

            if (isAssignedToTemplate && question.Type != request.Type)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Update.TypeCannotChangeAfterTemplateAssignment",
                    Message: ErrorMessage.UpdateGlobalQuestion_Type_CannotChangeAfterTemplateAssignment,
                    Type: ErrorType.Validation));
            }

            var currentOptions = await _questionOptionReadRepository.ListAsync(
                new GetGlobalQuestionOptionsForUpdateSpec(question.Id),
                cancellationToken);

            if (request.Type == QuestionType.SingleChoice)
            {
                var syncResult = await SyncSingleChoiceOptionsAsync(
                    questionId: question.Id,
                    requestOptions: request.Options,
                    currentOptions: currentOptions,
                    currentApplicationUserId: currentApplicationUserId,
                    cancellationToken: cancellationToken);

                if (syncResult.IsFailure)
                {
                    return Result<UpdateGlobalQuestionResponse>.Fail(syncResult.Errors);
                }
            }
            else
            {
                var deactivateResult = await DeactivateAllCurrentOptionsAsync(
                    currentOptions,
                    cancellationToken);

                if (deactivateResult.IsFailure)
                {
                    return Result<UpdateGlobalQuestionResponse>.Fail(deactivateResult.Errors);
                }
            }

            question.Update(
                groupId: group.Id,
                textEn: request.TextEn,
                textAr: request.TextAr,
                type: request.Type);

            _questionWriteRepository.Update(question);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var activeOptions = request.Type == QuestionType.SingleChoice
                ? currentOptions
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Order)
                    .Select(x => new QuestionOptionResponse
                    {
                        OptionId = x.Id,
                        QuestionId = x.QuestionId,
                        TextEn = x.TextEn,
                        TextAr = x.TextAr,
                        Order = x.Order,
                        Value = x.Value,
                        IsActive = x.IsActive
                    })
                    .ToArray()
                : Array.Empty<QuestionOptionResponse>();

            var response = new UpdateGlobalQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,
                IsEditable = question.Scope == QuestionScope.Global,
                TextEn = question.TextEn,
                TextAr = question.TextAr,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                IsActive = question.IsActive,
                Options = activeOptions
            };

            return Result<UpdateGlobalQuestionResponse>.Ok(response);
        }

        private async Task<Result> SyncSingleChoiceOptionsAsync(
            Guid questionId,
            IReadOnlyCollection<UpdateGlobalQuestionOptionCommandItem> requestOptions,
            IReadOnlyCollection<QuestionOption> currentOptions,
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var requestedExistingOptionIds = requestOptions
                .Where(x => x.OptionId.HasValue)
                .Select(x => x.OptionId.GetValueOrDefault())
                .ToArray();

            var hasDuplicatedOptionIds = requestedExistingOptionIds
                .GroupBy(x => x)
                .Any(x => x.Count() > 1);

            if (hasDuplicatedOptionIds)
            {
                return Result.Fail(new Error(
                    Code: "GlobalQuestions.Update.OptionIdDuplicated",
                    Message: ErrorMessage.UpdateGlobalQuestion_OptionId_Duplicated,
                    Type: ErrorType.Validation));
            }

            var currentOptionsById = currentOptions.ToDictionary(x => x.Id);

            var hasOptionNotBelongToQuestion = requestedExistingOptionIds
                .Any(optionId => !currentOptionsById.ContainsKey(optionId));

            if (hasOptionNotBelongToQuestion)
            {
                return Result.Fail(new Error(
                    Code: "GlobalQuestions.Update.OptionNotBelongToQuestion",
                    Message: ErrorMessage.UpdateGlobalQuestion_Option_NotBelongToQuestion,
                    Type: ErrorType.Validation));
            }

            var removedOptionIds = currentOptions
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .Except(requestedExistingOptionIds)
                .ToArray();

            if (removedOptionIds.Length > 0)
            {
                var usedConditions = await _templateQuestionConditionReadRepository.ListAsync(
                    new GetActiveTemplateQuestionConditionsByOptionIdsForUpdateGlobalQuestionSpec(
                        removedOptionIds),
                    cancellationToken);

                if (usedConditions.Count > 0)
                {
                    return Result.Fail(new Error(
                        Code: "GlobalQuestions.Update.OptionUsedInCondition",
                        Message: ErrorMessage.UpdateGlobalQuestion_Option_UsedInCondition,
                        Type: ErrorType.Validation));
                }

                foreach (var optionId in removedOptionIds)
                {
                    currentOptionsById[optionId].Deactivate();
                    _questionOptionWriteRepository.Update(currentOptionsById[optionId]);
                }
            }

            foreach (var requestOption in requestOptions)
            {
                if (requestOption.OptionId.HasValue)
                {
                    var option = currentOptionsById[requestOption.OptionId.GetValueOrDefault()];

                    option.Update(
                        textEn: requestOption.TextEn,
                        textAr: requestOption.TextAr,
                        order: requestOption.Order,
                        value: requestOption.Value);

                    _questionOptionWriteRepository.Update(option);

                    continue;
                }

                var newOption = QuestionOption.Create(
                    questionId: questionId,
                    textEn: requestOption.TextEn,
                    textAr: requestOption.TextAr,
                    order: requestOption.Order,
                    value: requestOption.Value,
                    createdByApplicationUserId: currentApplicationUserId);

                await _questionOptionWriteRepository.AddAsync(newOption, cancellationToken);
            }

            return Result.Ok();
        }

        private async Task<Result> DeactivateAllCurrentOptionsAsync(
            IReadOnlyCollection<QuestionOption> currentOptions,
            CancellationToken cancellationToken)
        {
            var activeOptionIds = currentOptions
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToArray();

            if (activeOptionIds.Length == 0)
            {
                return Result.Ok();
            }

            var usedConditions = await _templateQuestionConditionReadRepository.ListAsync(
                new GetActiveTemplateQuestionConditionsByOptionIdsForUpdateGlobalQuestionSpec(
                    activeOptionIds),
                cancellationToken);

            if (usedConditions.Count > 0)
            {
                return Result.Fail(new Error(
                    Code: "GlobalQuestions.Update.OptionUsedInCondition",
                    Message: ErrorMessage.UpdateGlobalQuestion_Option_UsedInCondition,
                    Type: ErrorType.Validation));
            }

            foreach (var option in currentOptions.Where(x => x.IsActive))
            {
                option.Deactivate();
                _questionOptionWriteRepository.Update(option);
            }

            return Result.Ok();
        }
    }
}