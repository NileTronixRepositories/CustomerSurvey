using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    internal sealed class GetBranchesPaginationQueryHandler
        : IQueryHandler<GetBranchesPaginationQuery, Pagination<BranchPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetBranchesPaginationQueryHandler(
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

        public async Task<Result<Pagination<BranchPaginationItemResponse>>> Handle(
            GetBranchesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<BranchPaginationItemResponse>>.Fail(new Error(
                    Code: "Branches.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<Pagination<BranchPaginationItemResponse>>.Fail(new Error(
                    Code: "Branches.Pagination.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetBranchesPagination_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            request.SearchText ??= string.Empty;

            var spec = new GetBranchesPaginationSpec(request);

            var (items, totalCount) = await _branchReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<BranchPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<BranchPaginationItemResponse>>.Ok(response);
        }
    }
}