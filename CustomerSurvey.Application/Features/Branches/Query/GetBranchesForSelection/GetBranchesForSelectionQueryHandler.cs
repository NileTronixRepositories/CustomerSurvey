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

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesForSelection
{
    internal sealed class GetBranchesForSelectionQueryHandler
          : IQueryHandler<GetBranchesForSelectionQuery, IReadOnlyCollection<BranchSelectionResponse>>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetBranchesForSelectionQueryHandler(
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<BranchSelectionResponse>>> Handle(
            GetBranchesForSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<BranchSelectionResponse>>.Fail(new Error(
                    Code: "Branches.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<IReadOnlyCollection<BranchSelectionResponse>>.Fail(new Error(
                    Code: "Branches.Selection.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetBranchesSelection_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var spec = new GetBranchesForSelectionSpec();

            var branches = await _branchReadRepository.ListAsync(
                spec,
                cancellationToken);

            return Result<IReadOnlyCollection<BranchSelectionResponse>>.Ok(branches);
        }
    }
}