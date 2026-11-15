using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class UpdateGlobalQuestionCommandHandler
         : ICommandHandler<UpdateGlobalQuestionCommand, UpdateGlobalQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly QuestionDefinitionUpdater _definitionUpdater;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public UpdateGlobalQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            QuestionDefinitionUpdater definitionUpdater,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _definitionUpdater = definitionUpdater
                ?? throw new ArgumentNullException(nameof(definitionUpdater));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
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

            var options = request.Type == QuestionType.SingleChoice
                ? request.Options.Select(x => new QuestionOptionCommandItem
                {
                    OptionId = x.OptionId,
                    TextEn = x.TextEn,
                    TextAr = x.TextAr,
                    Order = x.Order,
                    Value = x.Value
                }).ToArray()
                : Array.Empty<QuestionOptionCommandItem>();

            var update = await _definitionUpdater.UpdateAsync(
                question, group.Id, request.TextEn, request.TextAr, request.Type,
                options, currentApplicationUserId, cancellationToken);

            if (update.IsFailure)
            {
                return Result<UpdateGlobalQuestionResponse>.Fail(update.Errors);
            }

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
                Options = update.Value
            };

            return Result<UpdateGlobalQuestionResponse>.Ok(response);
        }
    }
}
