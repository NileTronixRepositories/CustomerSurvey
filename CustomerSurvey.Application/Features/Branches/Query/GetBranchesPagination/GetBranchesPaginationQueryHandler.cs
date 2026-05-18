using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    internal sealed class GetBranchesPaginationQueryHandler
        : IQueryHandler<GetBranchesPaginationQuery, Pagination<BranchPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetBranchesPaginationQueryHandler(
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser)
        {
            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

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

            var creatorApplicationUserIds = items
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<BranchPaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<BranchPaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetBranchCreatorsForBranchesPaginationSpec(creatorApplicationUserIds),
                    cancellationToken);
            }

            var creatorsByApplicationUserId = creators.ToDictionary(
                x => x.ApplicationUserId,
                x => x);

            var responseItems = items
                .Select(x =>
                {
                    creatorsByApplicationUserId.TryGetValue(
                        x.CreatedByApplicationUserId,
                        out var creator);

                    return new BranchPaginationItemResponse
                    {
                        Id = x.Id,
                        NameEn = x.NameEn,
                        NameAr = x.NameAr,
                        Code = x.Code,
                        Address = x.Address,
                        IsActive = x.IsActive,
                        CreatedBy = creator is null
                            ? null
                            : new BranchPaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = x.CreatedOnUtc
                    };
                })
                .ToArray();

            var response = new Pagination<BranchPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

            return Result<Pagination<BranchPaginationItemResponse>>.Ok(response);
        }
    }
}