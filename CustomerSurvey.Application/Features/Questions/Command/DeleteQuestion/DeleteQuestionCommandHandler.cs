using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    internal sealed class DeleteQuestionCommandHandler
        : ICommandHandler<DeleteQuestionCommand, DeleteQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteQuestionResponse>> Handle(
            DeleteQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteQuestionResponse>.Fail(new Error(
                    Code: "Questions.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<DeleteQuestionResponse>.Fail(currentBranchScope.Errors);
            }

            var branchId = currentBranchScope.Value.BranchId;

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetQuestionForDeleteSpec(
                    questionId: request.QuestionId,
                    branchId: branchId),
                cancellationToken);

            if (question is null)
            {
                return Result<DeleteQuestionResponse>.Fail(new Error(
                    Code: "Questions.Delete.QuestionNotFound",
                    Message: ErrorMessage.DeleteQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!question.IsActive)
            {
                return Result<DeleteQuestionResponse>.Fail(new Error(
                    Code: "Questions.Delete.QuestionAlreadyInactive",
                    Message: ErrorMessage.DeleteQuestion_Question_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            question.Deactivate();

            _questionWriteRepository.Update(question);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,

                // This endpoint deletes Branch Questions only.
                IsEditable = question.Scope == QuestionScope.Branch,

                IsActive = question.IsActive
            };

            return Result<DeleteQuestionResponse>.Ok(response);
        }

    }
}
