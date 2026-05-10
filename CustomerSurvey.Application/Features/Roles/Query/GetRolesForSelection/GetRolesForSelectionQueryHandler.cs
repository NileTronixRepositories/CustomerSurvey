using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchUsers;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Roles.Query.GetRolesForSelection
{
    internal sealed class GetRolesForSelectionQueryHandler
       : IQueryHandler<GetRolesForSelectionQuery, IReadOnlyCollection<RoleSelectionResponse>>
    {
        private readonly IWriteReadRepository<Role> _roleReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetRolesForSelectionQueryHandler(
            IWriteReadRepository<Role> roleReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            ICurrentUser currentUser)
        {
            _roleReadRepository = roleReadRepository
                ?? throw new ArgumentNullException(nameof(roleReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<RoleSelectionResponse>>> Handle(
            GetRolesForSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<RoleSelectionResponse>>.Fail(new Error(
                    Code: "Roles.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdminExists = await _branchAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentBranchAdminExists)
            {
                return Result<IReadOnlyCollection<RoleSelectionResponse>>.Fail(new Error(
                    Code: "Roles.Selection.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.GetRolesSelection_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var roles = await _roleReadRepository.ListAsync(
                new GetRolesForSelectionSpec(BranchUserAssignableRoles.Names),
                cancellationToken);

            if (roles.Count != BranchUserAssignableRoles.Names.Count)
            {
                return Result<IReadOnlyCollection<RoleSelectionResponse>>.Fail(new Error(
                    Code: "Roles.Selection.RequiredRolesNotSeeded",
                    Message: ErrorMessage.GetRolesSelection_RequiredRoles_NotSeeded,
                    Type: ErrorType.Conflict));
            }

            return Result<IReadOnlyCollection<RoleSelectionResponse>>.Ok(roles);
        }
    }
}