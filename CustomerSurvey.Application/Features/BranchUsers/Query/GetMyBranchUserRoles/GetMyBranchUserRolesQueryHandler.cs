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

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles
{
    internal sealed class GetMyBranchUserRolesQueryHandler
      : IQueryHandler<GetMyBranchUserRolesQuery, GetMyBranchUserRolesResponse>
    {
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetMyBranchUserRolesQueryHandler(
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            ICurrentUser currentUser)
        {
            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetMyBranchUserRolesResponse>> Handle(
            GetMyBranchUserRolesQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetMyBranchUserRolesResponse>.Fail(new Error(
                    Code: "BranchUsers.MyRoles.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForMyRolesSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchUser is null)
            {
                return Result<GetMyBranchUserRolesResponse>.Fail(new Error(
                    Code: "BranchUsers.MyRoles.CurrentBranchUserNotFound",
                    Message: ErrorMessage.GetMyBranchUserRoles_CurrentBranchUser_NotFound,
                    Type: ErrorType.Security));
            }

            var roles = await _userRoleReadRepository.ListAsync(
                new GetRolesForMyBranchUserSpec(currentApplicationUserId),
                cancellationToken);

            if (roles.Count == 0)
            {
                return Result<GetMyBranchUserRolesResponse>.Fail(new Error(
                    Code: "BranchUsers.MyRoles.UserHasNoRoles",
                    Message: ErrorMessage.GetMyBranchUserRoles_UserHasNoRoles,
                    Type: ErrorType.Security));
            }

            var response = new GetMyBranchUserRolesResponse
            {
                ApplicationUserId = currentBranchUser.ApplicationUserId,
                BranchUserId = currentBranchUser.BranchUserId,
                BranchId = currentBranchUser.BranchId,
                Roles = roles
            };

            return Result<GetMyBranchUserRolesResponse>.Ok(response);
        }
    }
}