using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    internal sealed class AssignRolesToBranchUserCommandHandler
        : ICommandHandler<AssignRolesToBranchUserCommand, AssignRolesToBranchUserResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly IWriteRepository<UserRole> _userRoleWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AssignRolesToBranchUserCommandHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<Role> roleReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            IWriteRepository<UserRole> userRoleWriteRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _roleReadRepository = roleReadRepository
                ?? throw new ArgumentNullException(nameof(roleReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _userRoleWriteRepository = userRoleWriteRepository
                ?? throw new ArgumentNullException(nameof(userRoleWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<AssignRolesToBranchUserResponse>> Handle(
            AssignRolesToBranchUserCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForAssignRolesSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.AssignRolesToBranchUser_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var targetBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetTargetBranchUserForAssignRolesSpec(request.ApplicationUserId),
                cancellationToken);

            if (targetBranchUser is null)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.BranchUserNotFound",
                    Message: ErrorMessage.AssignRolesToBranchUser_BranchUser_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (targetBranchUser.BranchId != currentBranchAdmin.BranchId)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.BranchScopeMismatch",
                    Message: ErrorMessage.AssignRolesToBranchUser_BranchScope_Mismatch,
                    Type: ErrorType.Security));
            }

            var requestedRoleIds = request.RoleIds
                .Distinct()
                .ToArray();

            var roles = await _roleReadRepository.ListAsync(
                new GetRolesForAssignRolesToBranchUserSpec(requestedRoleIds),
                cancellationToken);

            if (roles.Count != requestedRoleIds.Length)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.RoleNotFound",
                    Message: ErrorMessage.AssignRolesToBranchUser_Role_NotFound,
                    Type: ErrorType.NotFound));
            }

            var hasNotAllowedRole = roles.Any(x => !BranchUserAssignableRoles.IsAllowed(x.Name));

            if (hasNotAllowedRole)
            {
                return Result<AssignRolesToBranchUserResponse>.Fail(new Error(
                    Code: "BranchUsers.AssignRoles.RoleNotAllowed",
                    Message: ErrorMessage.AssignRolesToBranchUser_Role_NotAllowed,
                    Type: ErrorType.Security));
            }

            var existingUserRoles = await _userRoleReadRepository.ListAsync(
                new GetExistingUserRolesForAssignRolesSpec(targetBranchUser.ApplicationUserId),
                cancellationToken);

            var existingRoleIds = existingUserRoles
                .Select(x => x.RoleId)
                .ToHashSet();

            var requestedRoleIdsHashSet = requestedRoleIds
                .ToHashSet();

            var userRolesToRemove = existingUserRoles
                .Where(x => !requestedRoleIdsHashSet.Contains(x.RoleId))
                .ToArray();

            foreach (var userRole in userRolesToRemove)
            {
                _userRoleWriteRepository.Delete(userRole);
            }

            var roleIdsToAdd = requestedRoleIds
                .Where(x => !existingRoleIds.Contains(x))
                .ToArray();

            foreach (var roleId in roleIdsToAdd)
            {
                var userRole = UserRole.Create(
                    applicationUserId: targetBranchUser.ApplicationUserId,
                    roleId: roleId,
                    createdByApplicationUserId: currentApplicationUserId);

                await _userRoleWriteRepository.AddAsync(userRole, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AssignRolesToBranchUserResponse
            {
                BranchUserId = targetBranchUser.BranchUserId,
                ApplicationUserId = targetBranchUser.ApplicationUserId,
                BranchId = targetBranchUser.BranchId,
                Roles = roles
                    .Select(x => new AssignedBranchUserRoleResponse
                    {
                        RoleId = x.RoleId,
                        Name = x.Name
                    })
                    .ToArray()
            };

            return Result<AssignRolesToBranchUserResponse>.Ok(response);
        }
    }
}