using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using CustomerUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Shared.BranchScope
{
    internal sealed class CurrentBranchScopeResolver : ICurrentBranchScopeResolver
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;

        public CurrentBranchScopeResolver(
            ICurrentUser currentUser,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository)
        {
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _branchAreaReadRepository = branchAreaReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));

            _branchAreaBranchReadRepository = branchAreaBranchReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));
        }

        public async Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken = default)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "CurrentBranchScope.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var applicationUserId = _currentUser.UserId.Value;

            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminScopeSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return Result<CurrentBranchScope>.Ok(new CurrentBranchScope
                {
                    ApplicationUserId = applicationUserId,
                    ActorProfileId = branchAdmin.BranchAdminId,
                    ActorType = CustomerUserType.BranchAdmin,
                    BranchId = branchAdmin.BranchId
                });
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserScopeSpec(applicationUserId),
                cancellationToken);

            if (branchUser is not null)
            {
                return Result<CurrentBranchScope>.Ok(new CurrentBranchScope
                {
                    ApplicationUserId = applicationUserId,
                    ActorProfileId = branchUser.BranchUserId,
                    ActorType = CustomerUserType.BranchUser,
                    BranchId = branchUser.BranchId
                });
            }

            if (_currentUser.UserTypeValue != (int)CustomerUserType.BranchArea)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "CurrentBranchScope.CurrentBranchActorNotFound",
                    Message: ErrorMessage.CurrentBranchScope_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            if (!_currentUser.ActiveBranchId.HasValue)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "BranchArea.SelectedBranchRequired",
                    Message: ErrorMessage.BranchArea_SelectedBranch_Required,
                    Type: ErrorType.Security));
            }

            var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAreaScopeSpec(applicationUserId),
                cancellationToken);

            if (branchArea is null || !branchArea.IsActive)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "BranchArea.CurrentProfileNotFound",
                    Message: ErrorMessage.BranchArea_CurrentProfile_NotFound,
                    Type: ErrorType.NotFound));
            }

            var hasAnyAssignedBranches = await _branchAreaBranchReadRepository.AnyAsync(
                x => x.BranchAreaId == branchArea.BranchAreaId,
                cancellationToken);

            if (!hasAnyAssignedBranches)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "BranchArea.NoBranchesAssigned",
                    Message: ErrorMessage.BranchArea_NoBranches_Assigned,
                    Type: ErrorType.Security));
            }

            var activeBranchId = _currentUser.ActiveBranchId.Value;

            var assignedBranch = await _branchAreaBranchReadRepository.FirstOrDefaultAsync(
                new GetAssignedBranchAreaBranchScopeSpec(branchArea.BranchAreaId, activeBranchId),
                cancellationToken);

            if (assignedBranch is null)
            {
                return Result<CurrentBranchScope>.Fail(new Error(
                    Code: "BranchArea.SelectedBranchNotAllowed",
                    Message: ErrorMessage.BranchArea_SelectedBranch_NotAllowed,
                    Type: ErrorType.Security));
            }

            return Result<CurrentBranchScope>.Ok(new CurrentBranchScope
            {
                ApplicationUserId = applicationUserId,
                ActorProfileId = branchArea.BranchAreaId,
                ActorType = CustomerUserType.BranchArea,
                BranchId = activeBranchId
            });
        }
    }
}
