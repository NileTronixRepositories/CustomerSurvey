using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class RestoreQuestionCommandHandler
        : ICommandHandler<RestoreQuestionCommand, RestoreQuestionResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteRepository<Question> _questionWriteRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreQuestionCommandHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteRepository<Question> questionWriteRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionWriteRepository = questionWriteRepository
                ?? throw new ArgumentNullException(nameof(questionWriteRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreQuestionResponse>> Handle(
            RestoreQuestionCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.CurrentBranchActorNotFound",
                    Message: ErrorMessage.RestoreQuestion_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetQuestionForRestoreSpec(
                    questionId: request.QuestionId,
                    branchId: branchId),
                cancellationToken);

            if (question is null)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.QuestionNotFound",
                    Message: ErrorMessage.RestoreQuestion_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (question.IsActive)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.QuestionAlreadyActive",
                    Message: ErrorMessage.RestoreQuestion_Question_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForRestoreQuestionSpec(
                    groupId: question.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.QuestionGroupNotFound",
                    Message: ErrorMessage.RestoreQuestion_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!group.IsActive)
            {
                return Result<RestoreQuestionResponse>.Fail(new Error(
                    Code: "Questions.Restore.QuestionGroupInactive",
                    Message: ErrorMessage.RestoreQuestion_QuestionGroup_Inactive,
                    Type: ErrorType.Validation));
            }

            question.Activate();

            _questionWriteRepository.Update(question);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RestoreQuestionResponse
            {
                QuestionId = question.Id,
                BranchId = question.BranchId,
                GroupId = question.GroupId,
                GroupBranchId = group.BranchId,
                Scope = question.Scope,
                ScopeName = question.Scope.ToString(),
                IsGlobal = question.Scope == QuestionScope.Global,

                // This endpoint restores Branch Questions only.
                IsEditable = question.Scope == QuestionScope.Branch,

                IsActive = question.IsActive
            };

            return Result<RestoreQuestionResponse>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForRestoreQuestionSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForRestoreQuestionSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}