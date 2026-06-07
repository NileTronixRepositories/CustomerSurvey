using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.RestoreBranchArea
{
    internal sealed class RestoreBranchAreaCommandHandler
        : ICommandHandler<RestoreBranchAreaCommand, RestoreBranchAreaResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreBranchAreaCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAreaReadRepository = branchAreaReadRepository ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreBranchAreaResponse>> Handle(RestoreBranchAreaCommand request, CancellationToken cancellationToken)
        {
            var guard = await EnsureCurrentSuperAdminAsync(cancellationToken);
            if (guard.IsFailure)
            {
                return Result<RestoreBranchAreaResponse>.Fail(guard.Errors);
            }

            var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetBranchAreaForManagementSpec(request.BranchAreaId),
                cancellationToken);

            if (branchArea is null)
            {
                return Result<RestoreBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Restore.BranchAreaNotFound",
                    Message: ErrorMessage.AssignBranchAreaBranches_BranchArea_NotFound,
                    Type: ErrorType.NotFound));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                branchArea.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<RestoreBranchAreaResponse>.Fail(new Error(
                    Code: "BranchAreas.Restore.ApplicationUserNotFound",
                    Message: ErrorMessage.BranchArea_CurrentProfile_NotFound,
                    Type: ErrorType.NotFound));
            }

            applicationUser.Activate();
            _applicationUserWriteRepository.Update(applicationUser);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreBranchAreaResponse>.Ok(new RestoreBranchAreaResponse
            {
                BranchAreaId = branchArea.BranchAreaId,
                ApplicationUserId = applicationUser.Id,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result> EnsureCurrentSuperAdminAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "BranchAreas.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var exists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == _currentUser.UserId.Value,
                cancellationToken);

            return exists
                ? Result.Ok()
                : Result.Fail(new Error(
                    Code: "BranchAreas.Restore.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
