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

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartment
{
    internal sealed class CreateDepartmentCommandHandler
      : ICommandHandler<CreateDepartmentCommand, CreateDepartmentResponse>
    {
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteRepository<Department> _departmentWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDepartmentCommandHandler(
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

        public async Task<Result<CreateDepartmentResponse>> Handle(
            CreateDepartmentCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<CreateDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateDepartment_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedNameEn = request.NameEn.Trim();

            var normalizedNameAr = string.IsNullOrWhiteSpace(request.NameAr)
                ? null
                : request.NameAr.Trim();

            var nameEnExists = await _departmentReadRepository.AnyAsync(
                x => x.NameEn == normalizedNameEn,
                cancellationToken);

            if (nameEnExists)
            {
                return Result<CreateDepartmentResponse>.Fail(new Error(
                    Code: "Departments.Create.NameEnAlreadyExists",
                    Message: ErrorMessage.CreateDepartment_NameEn_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            if (!string.IsNullOrWhiteSpace(normalizedNameAr))
            {
                var nameArExists = await _departmentReadRepository.AnyAsync(
                    x => x.NameAr == normalizedNameAr,
                    cancellationToken);

                if (nameArExists)
                {
                    return Result<CreateDepartmentResponse>.Fail(new Error(
                        Code: "Departments.Create.NameArAlreadyExists",
                        Message: ErrorMessage.CreateDepartment_NameAr_AlreadyExists,
                        Type: ErrorType.Validation));
                }
            }

            var department = Department.Create(
                nameEn: normalizedNameEn,
                nameAr: normalizedNameAr,
                createdByApplicationUserId: currentApplicationUserId);

            await _departmentWriteRepository.AddAsync(
                department,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateDepartmentResponse
            {
                DepartmentId = department.Id,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                IsActive = department.IsActive
            };

            return Result<CreateDepartmentResponse>.Ok(response);
        }
    }
}