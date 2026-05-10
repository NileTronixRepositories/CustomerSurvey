using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin
{
    internal sealed class CreateDepartmentAdminCommandHandler
        : ICommandHandler<CreateDepartmentAdminCommand, CreateDepartmentAdminResponse>
    {
        private const string DepartmentAdministratorRoleName = "Department Administrator";

        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<Department> _departmentReadRepository;

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;

        private readonly IWriteRepository<DepartmentAdmin> _departmentAdminWriteRepository;

        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public CreateDepartmentAdminCommandHandler(
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteRepository<DepartmentAdmin> departmentAdminWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));

            _departmentAdminWriteRepository = departmentAdminWriteRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminWriteRepository));

            _roleReadRepository = roleReadRepository
                ?? throw new ArgumentNullException(nameof(roleReadRepository));

            _userRoleWriteRepository = userRoleWriteRepository
                ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<CreateDepartmentAdminResponse>> Handle(
            CreateDepartmentAdminCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateDepartmentAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var department = await _departmentReadRepository.FirstOrDefaultAsync(
                new GetDepartmentForCreateDepartmentAdminSpec(request.DepartmentId),
                cancellationToken);

            if (department is null)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.DepartmentNotFound",
                    Message: ErrorMessage.CreateDepartmentAdmin_Department_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim();

            var userNameExists = await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

            if (userNameExists)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.UserNameAlreadyExists",
                    Message: ErrorMessage.CreateDepartmentAdmin_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.EmailAlreadyExists",
                    Message: ErrorMessage.CreateDepartmentAdmin_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var departmentAdministratorRole = await _roleReadRepository.FirstOrDefaultAsync(
                new GetRoleByNameForCreateDepartmentAdminSpec(DepartmentAdministratorRoleName),
                cancellationToken);

            if (departmentAdministratorRole is null)
            {
                return Result<CreateDepartmentAdminResponse>.Fail(new Error(
                    Code: "DepartmentAdmins.Create.DepartmentAdministratorRoleNotFound",
                    Message: ErrorMessage.CreateDepartmentAdmin_DepartmentAdministratorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            var passwordHash = _passwordHasher.HashPassword(
                user: null!,
                password: request.Password);

            var applicationUser = ApplicationUser.Create(
                userName: normalizedUserName,
                passwordHash: passwordHash,
                email: normalizedEmail,
                phoneNumber: request.PhoneNumber,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                userType: Domain.Enums.UserType.DepartmentAdmin,
                createdByApplicationUserId: currentApplicationUserId);

            var departmentAdmin = DepartmentAdmin.Create(
                applicationUserId: applicationUser.Id,
                departmentId: department.Id,
                createdByApplicationUserId: currentApplicationUserId);

            var userRole = UserRole.Create(
                applicationUserId: applicationUser.Id,
                roleId: departmentAdministratorRole.Id,
                createdByApplicationUserId: currentApplicationUserId);

            await _applicationUserWriteRepository.AddAsync(
                applicationUser,
                cancellationToken);

            await _departmentAdminWriteRepository.AddAsync(
                departmentAdmin,
                cancellationToken);

            await _userRoleWriteRepository.AddAsync(
                userRole,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateDepartmentAdminResponse
            {
                ApplicationUserId = applicationUser.Id,
                DepartmentAdminId = departmentAdmin.Id,
                DepartmentId = departmentAdmin.DepartmentId,
                NameEn = applicationUser.NameEn,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email ?? string.Empty
            };

            return Result<CreateDepartmentAdminResponse>.Ok(response);
        }
    }
}