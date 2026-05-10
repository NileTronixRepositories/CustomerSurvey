using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment
{
    internal sealed class DeleteDepartmentCommandHandler
        : ICommandHandler<DeleteDepartmentCommand, DeleteDepartmentResponse>
    {
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteRepository<Department> _departmentWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDepartmentCommandHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteRepository<Department> departmentWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _departmentWriteRepository = departmentWriteRepository
                ?? throw new ArgumentNullException(nameof(departmentWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteDepartmentResponse>> Handle(
            DeleteDepartmentCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<DeleteDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Delete.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.DeleteDepartment_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var department = await _departmentReadRepository.FirstOrDefaultAsync(
                new GetDepartmentForDeleteSpec(request.DepartmentId),
                cancellationToken);

            if (department is null)
            {
                return Result<DeleteDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Delete.DepartmentNotFound",
                    Message: ErrorMessage.DeleteDepartment_Department_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!department.IsActive)
            {
                return Result<DeleteDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Delete.DepartmentAlreadyInactive",
                    Message: ErrorMessage.DeleteDepartment_Department_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            department.Deactivate();

            _departmentWriteRepository.Update(department);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteDepartmentResponse
            {
                DepartmentId = department.Id,
                IsActive = department.IsActive
            };

            return Result<DeleteDepartmentResponse>.Ok(response);
        }
    }
}