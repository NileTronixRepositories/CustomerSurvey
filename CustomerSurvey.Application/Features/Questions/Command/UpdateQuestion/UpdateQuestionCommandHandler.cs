using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class UpdateQuestionCommandHandler
        : ICommandHandler<UpdateQuestionCommand, UpdateQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly QuestionDefinitionUpdater _definitionUpdater;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public UpdateQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            QuestionDefinitionUpdater definitionUpdater,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _definitionUpdater = definitionUpdater
                ?? throw new ArgumentNullException(nameof(definitionUpdater));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<UpdateQuestionResponse>> Handle(
            UpdateQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<UpdateQuestionResponse>.Fail(currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetQuestionForUpdateSpec(
                    questionId: request.QuestionId,
                    branchId: branchId),
                cancellationToken);

            if (question is null)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionNotFound",
                    Message: ErrorMessage.UpdateQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForUpdateQuestionSpec(
                    groupId: request.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionGroupNotFound",
                    Message: ErrorMessage.UpdateQuestion_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!group.IsActive)
            {
                return Result<UpdateQuestionResponse>.Fail(new Error(
                    Code: "Questions.Update.QuestionGroupInactive",
                    Message: ErrorMessage.UpdateQuestion_QuestionGroup_Inactive,
                    Type: ErrorType.Validation));
            }

            var update = await _definitionUpdater.UpdateAsync(
                question, group.Id, request.TextEn, request.TextAr, request.Type,
                request.Options, currentApplicationUserId, cancellationToken);

            if (update.IsFailure)
            {
                return Result<UpdateQuestionResponse>.Fail(update.Errors);
            }

            var response = new UpdateQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,

                // This endpoint updates Branch Questions only.
                IsEditable = question.Scope == QuestionScope.Branch,

                TextEn = question.TextEn,
                TextAr = question.TextAr,
                Type = question.Type,
                TypeName = question.Type.ToString(),
                IsActive = question.IsActive,
                Options = update.Value
            };

            return Result<UpdateQuestionResponse>.Ok(response);
        }
    }
}
