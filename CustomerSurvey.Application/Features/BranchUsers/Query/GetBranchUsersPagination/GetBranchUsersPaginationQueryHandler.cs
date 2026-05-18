using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination
{
    internal sealed class GetBranchUsersPaginationQueryHandler
       : IQueryHandler<GetBranchUsersPaginationQuery, Pagination<BranchUserPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetBranchUsersPaginationQueryHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<BranchUserPaginationItemResponse>>> Handle(
            GetBranchUsersPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<BranchUserPaginationItemResponse>>.Fail(new Error(
                    Code: "BranchUsers.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForBranchUsersPaginationSpec(_currentUser.UserId.Value),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<Pagination<BranchUserPaginationItemResponse>>.Fail(new Error(
                    Code: "BranchUsers.Pagination.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.GetBranchUsersPagination_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            request.SearchText ??= string.Empty;

            var spec = new GetBranchUsersPaginationSpec(
                currentBranchAdmin.BranchId,
                request);

            var (users, totalCount) = await _branchUserReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var applicationUserIds = users
                .Select(x => x.ApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<BranchUserRoleForPaginationDto> userRoles;

            if (applicationUserIds.Length == 0)
            {
                userRoles = Array.Empty<BranchUserRoleForPaginationDto>();
            }
            else
            {
                userRoles = await _userRoleReadRepository.ListAsync(
                    new GetBranchUserRolesForPaginationSpec(applicationUserIds),
                    cancellationToken);
            }

            var rolesByApplicationUserId = userRoles
                .GroupBy(x => x.ApplicationUserId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<BranchUserPaginationRoleResponse>)x
                        .Select(role => new BranchUserPaginationRoleResponse
                        {
                            RoleId = role.RoleId,
                            Name = role.RoleName
                        })
                        .ToArray());

            var creatorApplicationUserIds = users
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<BranchUserPaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<BranchUserPaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetBranchUserCreatorsForBranchUsersPaginationSpec(creatorApplicationUserIds),
                    cancellationToken);
            }

            var creatorsByApplicationUserId = creators.ToDictionary(
                x => x.ApplicationUserId,
                x => x);

            var items = users
                .Select(user =>
                {
                    creatorsByApplicationUserId.TryGetValue(
                        user.CreatedByApplicationUserId,
                        out var creator);

                    return new BranchUserPaginationItemResponse
                    {
                        BranchUserId = user.BranchUserId,
                        ApplicationUserId = user.ApplicationUserId,
                        BranchId = user.BranchId,
                        NameEn = user.NameEn,
                        NameAr = user.NameAr,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        CreatedBy = creator is null
                            ? null
                            : new BranchUserPaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = user.CreatedOnUtc,
                        Roles = rolesByApplicationUserId.TryGetValue(
                            user.ApplicationUserId,
                            out var roles)
                                ? roles
                                : Array.Empty<BranchUserPaginationRoleResponse>()
                    };
                })
                .ToArray();

            var response = new Pagination<BranchUserPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<BranchUserPaginationItemResponse>>.Ok(response);
        }
    }
}