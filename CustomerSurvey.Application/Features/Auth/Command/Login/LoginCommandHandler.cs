using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Dto;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed class LoginCommandHandler
        : ICommandHandler<LoginCommand, UserTokenDto>
    {
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly IWriteReadRepository<RolePermission> _rolePermissionReadRepository;

        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly IWriteReadRepository<Operator> _operatorReadRepository;

        private readonly IJwtProvider _jwtProvider;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public LoginCommandHandler(
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            IWriteReadRepository<RolePermission> rolePermissionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            IWriteReadRepository<Operator> operatorReadRepository,
            IJwtProvider jwtProvider)
        {
            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _rolePermissionReadRepository = rolePermissionReadRepository
                ?? throw new ArgumentNullException(nameof(rolePermissionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _branchAreaReadRepository = branchAreaReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));

            _branchAreaBranchReadRepository = branchAreaBranchReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _jwtProvider = jwtProvider
                ?? throw new ArgumentNullException(nameof(jwtProvider));

            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task<Result<UserTokenDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var userNameOrEmail = request.UserNameOrEmail.Trim();

            var user = await _applicationUserReadRepository.FirstOrDefaultAsync(
                new GetApplicationUserForLoginSpec(userNameOrEmail),
                cancellationToken);

            if (user is null)
            {
                return InvalidCredentials();
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return InvalidCredentials();
            }

            var actorProfileExists = await ActorProfileExistsAsync(
                user.UserType,
                user.Id,
                cancellationToken);

            if (!actorProfileExists)
            {
                return Result<UserTokenDto>.Fail(new Error(
                    Code: "Auth.Login.ActorProfileNotFound",
                    Message: ErrorMessage.Login_ActorProfile_NotFound,
                    Type: ErrorType.NotFound));
            }

            var userRoles = await _userRoleReadRepository.ListAsync(
                new GetUserRolesForLoginSpec(user.Id),
                cancellationToken);

            if (userRoles.Count == 0)
            {
                return Result<UserTokenDto>.Fail(new Error(
                    Code: "Auth.Login.UserHasNoRoles",
                    Message: ErrorMessage.Login_UserHasNoRoles,
                    Type: ErrorType.Security));
            }

            var roleIds = userRoles
                .Select(x => x.RoleId)
                .Distinct()
                .ToArray();

            var roleNames = userRoles
                .Select(x => x.RoleName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var userPermissions = await _rolePermissionReadRepository.ListAsync(
                new GetUserPermissionsForLoginSpec(roleIds),
                cancellationToken);

            var permissions = userPermissions
                .Select(x => x.PermissionName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var token = await _jwtProvider.Generate(
                userId: user.Id,
                email: user.Email ?? string.Empty,
                phoneNumber: user.PhoneNumber ?? string.Empty,
                roleNames: roleNames,
                userType: user.UserType,
                permissions: permissions,
                cancellationToken: cancellationToken);

            if (user.UserType == UserType.BranchArea)
            {
                var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                    new GetBranchAreaForLoginSpec(user.Id),
                    cancellationToken);

                if (branchArea is null)
                {
                    return Result<UserTokenDto>.Fail(new Error(
                        Code: "Auth.Login.BranchAreaProfileNotFound",
                        Message: ErrorMessage.BranchArea_CurrentProfile_NotFound,
                        Type: ErrorType.NotFound));
                }

                var assignedBranches = await _branchAreaBranchReadRepository.ListAsync(
                    new GetBranchAreaBranchesForLoginSpec(branchArea.BranchAreaId),
                    cancellationToken);

                if (assignedBranches.Count == 0)
                {
                    return Result<UserTokenDto>.Fail(new Error(
                        Code: "Auth.Login.BranchAreaNoBranchesAssigned",
                        Message: ErrorMessage.BranchArea_NoBranches_Assigned,
                        Type: ErrorType.Security));
                }

                token = token with
                {
                    RequiresBranchSelection = true,
                    ActiveBranchId = null,
                    Branches = assignedBranches
                };
            }

            return Result<UserTokenDto>.Ok(token);
        }

        private async Task<bool> ActorProfileExistsAsync(
            UserType userType,
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            return userType switch
            {
                UserType.SuperAdmin => await _superAdminReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                UserType.BranchAdmin => await _branchAdminReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                UserType.BranchUser => await _branchUserReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                UserType.BranchArea => await _branchAreaReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                UserType.DepartmentAdmin => await _departmentAdminReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                UserType.Operator => await _operatorReadRepository.AnyAsync(
                    x => x.ApplicationUserId == applicationUserId,
                    cancellationToken),

                _ => false
            };
        }

        private static Result<UserTokenDto> InvalidCredentials()
        {
            return Result<UserTokenDto>.Fail(new Error(
                Code: "Auth.Login.InvalidCredentials",
                Message: ErrorMessage.Login_InvalidCredentials,
                Type: ErrorType.Security));
        }
    }
}
