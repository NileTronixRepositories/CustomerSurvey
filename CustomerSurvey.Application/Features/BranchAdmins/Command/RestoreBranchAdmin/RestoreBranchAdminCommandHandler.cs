using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    internal sealed class RestoreBranchAdminCommandHandler
        : ICommandHandler<RestoreBranchAdminCommand, RestoreBranchAdminResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreBranchAdminCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAdminReadRepository = branchAdminReadRepository ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreBranchAdminResponse>> Handle(
            RestoreBranchAdminCommand request,
            CancellationToken cancellationToken)
        {
            var guard = await EnsureCurrentSuperAdminAsync(cancellationToken);
            if (guard.IsFailure)
            {
                return Result<RestoreBranchAdminResponse>.Fail(guard.Errors);
            }

            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetBranchAdminForRestoreBranchAdminSpec(request.BranchAdminId),
                cancellationToken);

            if (branchAdmin is null)
            {
                return Result<RestoreBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Restore.TargetNotFound",
                    Message: ErrorMessage.RestoreBranchAdmin_BranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                branchAdmin.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<RestoreBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Restore.ApplicationUserNotFound",
                    Message: ErrorMessage.RestoreBranchAdmin_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (applicationUser.IsActive)
            {
                return Result<RestoreBranchAdminResponse>.Fail(new Error(
                    Code: "BranchAdmins.Restore.AlreadyActive",
                    Message: ErrorMessage.RestoreBranchAdmin_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            applicationUser.Activate();
            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreBranchAdminResponse>.Ok(new RestoreBranchAdminResponse
            {
                BranchAdminId = branchAdmin.BranchAdminId,
                ApplicationUserId = applicationUser.Id,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result> EnsureCurrentSuperAdminAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "BranchAdmins.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForRestoreBranchAdminSpec(currentApplicationUserId),
                cancellationToken);

            return currentApplicationUserActive && currentSuperAdmin is not null
                ? Result.Ok()
                : Result.Fail(new Error(
                    Code: "BranchAdmins.Restore.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.RestoreBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
