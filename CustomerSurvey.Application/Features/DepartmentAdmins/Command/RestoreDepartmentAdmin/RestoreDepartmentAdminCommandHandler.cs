using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin
{
    internal sealed class RestoreDepartmentAdminCommandHandler
        : ICommandHandler<RestoreDepartmentAdminCommand, RestoreDepartmentAdminResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreDepartmentAdminCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _departmentAdminReadRepository = departmentAdminReadRepository ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreDepartmentAdminResponse>> Handle(
            RestoreDepartmentAdminCommand request,
            CancellationToken cancellationToken)
        {
            var guard = await EnsureCurrentSuperAdminAsync(cancellationToken);
            if (guard.IsFailure)
            {
                return Result<RestoreDepartmentAdminResponse>.Fail(guard.Errors);
            }

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetDepartmentAdminForRestoreDepartmentAdminSpec(request.DepartmentAdminId),
                cancellationToken);

            if (departmentAdmin is null)
            {
                return Result<RestoreDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Restore.TargetNotFound",
                    Message: ErrorMessage.RestoreDepartmentAdmin_DepartmentAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var applicationUser = await _applicationUserReadRepository.GetByIdTrackedAsync(
                departmentAdmin.ApplicationUserId,
                cancellationToken);

            if (applicationUser is null)
            {
                return Result<RestoreDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Restore.ApplicationUserNotFound",
                    Message: ErrorMessage.RestoreDepartmentAdmin_ApplicationUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (applicationUser.IsActive)
            {
                return Result<RestoreDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Restore.AlreadyActive",
                    Message: ErrorMessage.RestoreDepartmentAdmin_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            applicationUser.Activate();
            _applicationUserWriteRepository.Update(applicationUser);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreDepartmentAdminResponse>.Ok(new RestoreDepartmentAdminResponse
            {
                DepartmentAdminId = departmentAdmin.DepartmentAdminId,
                ApplicationUserId = applicationUser.Id,
                IsActive = applicationUser.IsActive
            });
        }

        private async Task<Result> EnsureCurrentSuperAdminAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "DepartmentAdmins.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForRestoreDepartmentAdminSpec(currentApplicationUserId),
                cancellationToken);

            return currentApplicationUserActive && currentSuperAdmin is not null
                ? Result.Ok()
                : Result.Fail(new Error(
                    Code: "DepartmentAdmins.Restore.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.RestoreDepartmentAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
