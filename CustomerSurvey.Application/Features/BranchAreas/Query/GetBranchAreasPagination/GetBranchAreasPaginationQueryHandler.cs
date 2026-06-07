using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.BranchAreas.Shared;
using CustomerSurvey.Application.Features.BranchAreas.Shared.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreasPagination
{
    internal sealed class GetBranchAreasPaginationQueryHandler
        : IQueryHandler<GetBranchAreasPaginationQuery, Pagination<BranchAreaPaginationItemResponse>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchArea> _branchAreaReadRepository;
        private readonly IWriteReadRepository<BranchAreaBranch> _branchAreaBranchReadRepository;

        public GetBranchAreasPaginationQueryHandler(
            ICurrentUser currentUser,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchArea> branchAreaReadRepository,
            IWriteReadRepository<BranchAreaBranch> branchAreaBranchReadRepository)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _superAdminReadRepository = superAdminReadRepository ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
            _branchAreaReadRepository = branchAreaReadRepository ?? throw new ArgumentNullException(nameof(branchAreaReadRepository));
            _branchAreaBranchReadRepository = branchAreaBranchReadRepository ?? throw new ArgumentNullException(nameof(branchAreaBranchReadRepository));
        }

        public async Task<Result<Pagination<BranchAreaPaginationItemResponse>>> Handle(
            GetBranchAreasPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<BranchAreaPaginationItemResponse>>.Fail(new Error(
                    Code: "BranchAreas.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var superAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == _currentUser.UserId.Value,
                cancellationToken);

            if (!superAdminExists)
            {
                return Result<Pagination<BranchAreaPaginationItemResponse>>.Fail(new Error(
                    Code: "BranchAreas.Pagination.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.CreateBranchAdmin_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            request.SearchText ??= string.Empty;

            var (items, totalCount) = await _branchAreaReadRepository.ListWithCountAsync(
                new GetBranchAreasPaginationSpec(request),
                cancellationToken);

            var branchAreaIds = items
                .Select(x => x.BranchAreaId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<BranchAreaBranchListItemDto> branchItems =
                branchAreaIds.Length == 0
                    ? Array.Empty<BranchAreaBranchListItemDto>()
                    : await _branchAreaBranchReadRepository.ListAsync(
                        new GetBranchAreaBranchesByAreaIdsSpec(branchAreaIds),
                        cancellationToken);

            var branchesByAreaId = branchItems
                .GroupBy(x => x.BranchAreaId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<BranchAreaBranchItemResponse>)x
                        .Select(item => item.Branch)
                        .ToArray());

            var responseItems = items
                .Select(x => new BranchAreaPaginationItemResponse
                {
                    BranchAreaId = x.BranchAreaId,
                    ApplicationUserId = x.ApplicationUserId,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    UserName = x.UserName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    IsActive = x.IsActive,
                    CreatedOnUtc = x.CreatedOnUtc,
                    Branches = branchesByAreaId.TryGetValue(x.BranchAreaId, out var branches)
                        ? branches
                        : Array.Empty<BranchAreaBranchItemResponse>()
                })
                .ToArray();

            return Result<Pagination<BranchAreaPaginationItemResponse>>.Ok(
                new Pagination<BranchAreaPaginationItemResponse>(
                    currentPage: request.PageNumber,
                    pageSize: request.PageSize,
                    totalItems: totalCount,
                    data: responseItems));
        }
    }
}
