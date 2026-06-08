using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment
{
    internal sealed class RestoreDepartmentCommandHandler
        : ICommandHandler<RestoreDepartmentCommand, RestoreDepartmentResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteRepository<Department> _departmentWriteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreDepartmentCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteRepository<Department> departmentWriteRepository,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _departmentReadRepository = departmentReadRepository ?? throw new ArgumentNullException(nameof(departmentReadRepository));
            _departmentWriteRepository = departmentWriteRepository ?? throw new ArgumentNullException(nameof(departmentWriteRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreDepartmentResponse>> Handle(
            RestoreDepartmentCommand request,
            CancellationToken cancellationToken)
        {
            var guard = await EnsureCurrentSuperAdminAsync(cancellationToken);
            if (guard.IsFailure)
            {
                return Result<RestoreDepartmentResponse>.Fail(guard.Errors);
            }

            var department = await _departmentReadRepository.FirstOrDefaultAsync(
                new GetDepartmentForRestoreSpec(request.DepartmentId),
                cancellationToken);

            if (department is null)
            {
                return Result<RestoreDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Restore.DepartmentNotFound",
                    Message: ErrorMessage.RestoreDepartment_Department_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (department.IsActive)
            {
                return Result<RestoreDepartmentResponse>.Ok(new RestoreDepartmentResponse
                {
                    DepartmentId = department.Id,
                    IsActive = department.IsActive
                });
            }

            department.Activate();
            _departmentWriteRepository.Update(department);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreDepartmentResponse>.Ok(new RestoreDepartmentResponse
            {
                DepartmentId = department.Id,
                IsActive = department.IsActive
            });
        }

        private async Task<Result> EnsureCurrentSuperAdminAsync(CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result.Fail(new Error(
                    Code: "Departments.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForRestoreDepartmentSpec(currentApplicationUserId),
                cancellationToken);

            return currentApplicationUserActive && currentSuperAdmin is not null
                ? Result.Ok()
                : Result.Fail(new Error(
                    Code: "Departments.Restore.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.RestoreDepartment_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
        }
    }
}
