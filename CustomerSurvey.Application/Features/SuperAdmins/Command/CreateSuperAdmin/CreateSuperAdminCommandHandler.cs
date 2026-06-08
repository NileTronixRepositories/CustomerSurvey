using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using DomainUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin
{
    internal sealed class CreateSuperAdminCommandHandler
        : ICommandHandler<CreateSuperAdminCommand, CreateSuperAdminResponse>
    {
        private const string SystemAdministratorRoleName = "System Administrator";

        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteRepository<SuperAdmin> _superAdminWriteRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;

        public CreateSuperAdminCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteRepository<SuperAdmin> superAdminWriteRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            IPasswordService passwordService,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _superAdminWriteRepository = superAdminWriteRepository ?? throw new ArgumentNullException(nameof(superAdminWriteRepository));
            _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
            _applicationUserWriteRepository = applicationUserWriteRepository ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));
            _roleReadRepository = roleReadRepository ?? throw new ArgumentNullException(nameof(roleReadRepository));
            _userRoleWriteRepository = userRoleWriteRepository ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<CreateSuperAdminResponse>> Handle(
            CreateSuperAdminCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentApplicationUserActive = await _applicationUserReadRepository.AnyAsync(
                x => x.Id == currentApplicationUserId && x.IsActive,
                cancellationToken);

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForCreateSuperAdminSpec(currentApplicationUserId),
                cancellationToken);

            if (!currentApplicationUserActive || currentSuperAdmin is null)
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateSuperAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var duplicateUsers = await _applicationUserReadRepository.ListAsync(
                new GetApplicationUserByUserNameOrEmailSpec(normalizedUserName, normalizedEmail),
                cancellationToken);

            if (duplicateUsers.Any(x => x.UserName == normalizedUserName))
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.DuplicateUserName",
                    Message: ErrorMessage.CreateSuperAdmin_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            if (duplicateUsers.Any(x => x.Email == normalizedEmail))
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.DuplicateEmail",
                    Message: ErrorMessage.CreateSuperAdmin_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            if (!_passwordService.IsStrongPassword(request.Password))
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.InvalidPassword",
                    Message: ErrorMessage.CreateSuperAdmin_Password_Invalid,
                    Type: ErrorType.Validation));
            }

            var systemAdministratorRole = await _roleReadRepository.FirstOrDefaultAsync(
                new GetRoleByNameForCreateSuperAdminSpec(SystemAdministratorRoleName),
                cancellationToken);

            if (systemAdministratorRole is null)
            {
                return Result<CreateSuperAdminResponse>.Fail(new Error(
                    Code: "SuperAdmins.Create.SystemAdministratorRoleNotFound",
                    Message: ErrorMessage.CreateSuperAdmin_SystemAdministratorRole_NotFound,
                    Type: ErrorType.NotFound));
            }

            var passwordHash = await _passwordService.HashAsync(
                request.Password,
                cancellationToken);

            var applicationUser = ApplicationUser.Create(
                userName: normalizedUserName,
                email: normalizedEmail,
                nameEn: request.NameEn,
                nameAr: request.NameAr,
                phoneNumber: request.PhoneNumber,
                passwordHash: passwordHash,
                userType: DomainUserType.SuperAdmin,
                createdByApplicationUserId: currentApplicationUserId);

            var superAdmin = SuperAdmin.Create(
                applicationUserId: applicationUser.Id,
                createdByApplicationUserId: currentApplicationUserId);

            var userRole = UserRole.Create(
                applicationUserId: applicationUser.Id,
                roleId: systemAdministratorRole.Id,
                createdByApplicationUserId: currentApplicationUserId);

            await _applicationUserWriteRepository.AddAsync(applicationUser, cancellationToken);
            await _superAdminWriteRepository.AddAsync(superAdmin, cancellationToken);
            await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateSuperAdminResponse>.Ok(new CreateSuperAdminResponse
            {
                SuperAdminId = superAdmin.Id,
                ApplicationUserId = applicationUser.Id,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email,
                PhoneNumber = applicationUser.PhoneNumber,
                IsActive = applicationUser.IsActive
            });
        }
    }
}
