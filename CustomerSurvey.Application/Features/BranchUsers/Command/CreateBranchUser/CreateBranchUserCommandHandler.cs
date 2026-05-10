using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.CreateBranchUser
{
    internal sealed class CreateBranchUserCommandHandler
          : ICommandHandler<CreateBranchUserCommand, CreateBranchUserResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWriteRepository;
        private readonly IWriteRepository<BranchUser> _branchUserWriteRepository;
        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public CreateBranchUserCommandHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteRepository<ApplicationUser> applicationUserWriteRepository,
            IWriteRepository<BranchUser> branchUserWriteRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _applicationUserWriteRepository = applicationUserWriteRepository
                ?? throw new ArgumentNullException(nameof(applicationUserWriteRepository));

            _branchUserWriteRepository = branchUserWriteRepository
                ?? throw new ArgumentNullException(nameof(branchUserWriteRepository));

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

        public async Task<Result<CreateBranchUserResponse>> Handle(
            CreateBranchUserCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForCreateBranchUserSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.CreateBranchUser_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim();

            var userNameExists = await _applicationUserReadRepository.AnyAsync(
                x => x.UserName == normalizedUserName,
                cancellationToken);

            if (userNameExists)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.UserNameAlreadyExists",
                    Message: ErrorMessage.CreateBranchUser_UserName_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var emailExists = await _applicationUserReadRepository.AnyAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

            if (emailExists)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.EmailAlreadyExists",
                    Message: ErrorMessage.CreateBranchUser_Email_AlreadyExists,
                    Type: ErrorType.Validation));
            }

            var requestedRoleIds = request.RoleIds
                .Distinct()
                .ToArray();

            var roles = await _roleReadRepository.ListAsync(
                new GetRolesForCreateBranchUserSpec(requestedRoleIds),
                cancellationToken);

            if (roles.Count != requestedRoleIds.Length)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.RoleNotFound",
                    Message: ErrorMessage.CreateBranchUser_Role_NotFound,
                    Type: ErrorType.NotFound));
            }

            var hasNotAllowedRole = roles.Any(x => !BranchUserAssignableRoles.IsAllowed(x.Name));

            if (hasNotAllowedRole)
            {
                return Result<CreateBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.Create.RoleNotAllowed",
                    Message: ErrorMessage.CreateBranchUser_Role_NotAllowed,
                    Type: ErrorType.Security));
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
                userType: Domain.Enums.UserType.BranchUser,
                createdByApplicationUserId: currentApplicationUserId);

            var branchUser = BranchUser.Create(
                applicationUserId: applicationUser.Id,
                branchId: currentBranchAdmin.BranchId,
                createdByApplicationUserId: currentApplicationUserId);

            await _applicationUserWriteRepository.AddAsync(applicationUser, cancellationToken);
            await _branchUserWriteRepository.AddAsync(branchUser, cancellationToken);

            foreach (var roleId in requestedRoleIds)
            {
                var userRole = UserRole.Create(
                    applicationUserId: applicationUser.Id,
                    roleId: roleId,
                    createdByApplicationUserId: currentApplicationUserId);

                await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateBranchUserResponse
            {
                ApplicationUserId = applicationUser.Id,
                BranchUserId = branchUser.Id,
                BranchId = branchUser.BranchId,
                NameEn = applicationUser.NameEn,
                NameAr = applicationUser.NameAr,
                UserName = applicationUser.UserName,
                Email = applicationUser.Email ?? string.Empty,
                PhoneNumber = applicationUser.PhoneNumber,
                Roles = roles
                    .Select(x => new CreateBranchUserRoleResponse
                    {
                        RoleId = x.RoleId,
                        Name = x.Name
                    })
                    .ToArray()
            };

            return Result<CreateBranchUserResponse>.Ok(response);
        }
    }
}