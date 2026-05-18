using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed class RestoreQuestionGroupCommandHandler
        : ICommandHandler<RestoreQuestionGroupCommand, RestoreQuestionGroupResponse>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteRepository<QuestionGroup> _questionGroupWriteRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreQuestionGroupCommandHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteRepository<QuestionGroup> questionGroupWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionGroupWriteRepository = questionGroupWriteRepository
                ?? throw new ArgumentNullException(nameof(questionGroupWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreQuestionGroupResponse>> Handle(
            RestoreQuestionGroupCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<RestoreQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Restore.CurrentBranchActorNotFound",
                    Message: ErrorMessage.RestoreQuestionGroup_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var group = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForRestoreQuestionGroupSpec(
                    groupId: request.GroupId,
                    branchId: branchId),
                cancellationToken);

            if (group is null)
            {
                return Result<RestoreQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Restore.QuestionGroupNotFound",
                    Message: ErrorMessage.RestoreQuestionGroup_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (group.IsActive)
            {
                return Result<RestoreQuestionGroupResponse>.Fail(new Error(
                    Code: "QuestionGroups.Restore.QuestionGroupAlreadyActive",
                    Message: ErrorMessage.RestoreQuestionGroup_QuestionGroup_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            group.Activate();

            _questionGroupWriteRepository.Update(group);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RestoreQuestionGroupResponse
            {
                GroupId = group.Id,
                BranchId = group.BranchId,
                Scope = group.Scope,
                ScopeName = group.Scope.ToString(),
                IsGlobal = group.Scope == QuestionScope.Global,
                IsEditable = group.Scope == QuestionScope.Branch,
                NameEn = group.NameEn,
                NameAr = group.NameAr,
                IsActive = group.IsActive
            };

            return Result<RestoreQuestionGroupResponse>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}