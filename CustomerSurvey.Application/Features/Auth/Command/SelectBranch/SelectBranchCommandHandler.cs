using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Auth.Command.Login;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using CustomerUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    internal sealed class SelectBranchCommandHandler
        : ICommandHandler<SelectBranchCommand, SelectBranchResponse>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly IWriteReadRepository<RolePermission> _rolePermissionReadRepository;
        private readonly IJwtProvider _jwtProvider;

        public SelectBranchCommandHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            IWriteReadRepository<RolePermission> rolePermissionReadRepository,
            IJwtProvider jwtProvider)
        {
            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _branchAreaReadRepository = branchAreaReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));

            _branchAreaBranchReadRepository = branchAreaBranchReadRepository
                ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _rolePermissionReadRepository = rolePermissionReadRepository
                ?? throw new ArgumentNullException(nameof(rolePermissionReadRepository));

            _jwtProvider = jwtProvider
                ?? throw new ArgumentNullException(nameof(jwtProvider));
        }

        public async Task<Result<SelectBranchResponse>> Handle(
            SelectBranchCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            if (_currentUser.UserTypeValue != (int)CustomerUserType.BranchArea)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.CurrentUserNotBranchArea",
                    Message: ErrorMessage.SelectBranch_CurrentUser_NotBranchArea,
                    Type: ErrorType.Security));
            }

            var applicationUserId = _currentUser.UserId.Value;

            var branchArea = await _branchAreaReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAreaForSelectBranchSpec(applicationUserId),
                cancellationToken);

            if (branchArea is null || !branchArea.IsActive)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.BranchAreaProfileNotFound",
                    Message: ErrorMessage.BranchArea_CurrentProfile_NotFound,
                    Type: ErrorType.NotFound));
            }

            var hasAnyAssignedBranches = await _branchAreaBranchReadRepository.AnyAsync(
                x => x.BranchAreaId == branchArea.BranchAreaId,
                cancellationToken);

            if (!hasAnyAssignedBranches)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.BranchAreaNoBranchesAssigned",
                    Message: ErrorMessage.BranchArea_NoBranches_Assigned,
                    Type: ErrorType.Security));
            }

            var branchExists = await _branchReadRepository.AnyAsync(
                x => x.Id == request.BranchId && x.IsActive,
                cancellationToken);

            if (!branchExists)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.BranchNotFound",
                    Message: ErrorMessage.SelectBranch_Branch_NotFound,
                    Type: ErrorType.NotFound));
            }

            var selectedBranch = await _branchAreaBranchReadRepository.FirstOrDefaultAsync(
                new GetAssignedBranchForSelectBranchSpec(branchArea.BranchAreaId, request.BranchId),
                cancellationToken);

            if (selectedBranch is null)
            {
                return Result<SelectBranchResponse>.Fail(new Error(
                    Code: "Auth.SelectBranch.SelectedBranchNotAllowed",
                    Message: ErrorMessage.BranchArea_SelectedBranch_NotAllowed,
                    Type: ErrorType.Security));
            }

            var userRoles = await _userRoleReadRepository.ListAsync(
                new GetUserRolesForLoginSpec(applicationUserId),
                cancellationToken);

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
                userId: applicationUserId,
                email: branchArea.Email,
                phoneNumber: branchArea.PhoneNumber ?? string.Empty,
                roleNames: roleNames,
                userType: CustomerUserType.BranchArea,
                permissions: permissions,
                activeBranchId: selectedBranch.Id,
                cancellationToken: cancellationToken);

            return Result<SelectBranchResponse>.Ok(new SelectBranchResponse
            {
                Token = token.Token,
                UserType = CustomerUserType.BranchArea.ToString(),
                ActiveBranchId = selectedBranch.Id,
                SelectedBranch = selectedBranch
            });
        }
    }
}
