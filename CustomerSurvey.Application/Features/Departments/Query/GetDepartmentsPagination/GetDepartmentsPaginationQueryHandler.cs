using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination
{
    internal sealed class GetDepartmentsPaginationQueryHandler
       : IQueryHandler<GetDepartmentsPaginationQuery, Pagination<DepartmentPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Department> _departmentReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetDepartmentsPaginationQueryHandler(
            IWriteReadRepository<Department> departmentReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser)
        {
            _departmentReadRepository = departmentReadRepository
                ?? throw new ArgumentNullException(nameof(departmentReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<DepartmentPaginationItemResponse>>> Handle(
            GetDepartmentsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<DepartmentPaginationItemResponse>>.Fail(new Error(
                    Code: "Departments.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            if (!currentSuperAdminExists)
            {
                return Result<Pagination<DepartmentPaginationItemResponse>>.Fail(new Error(
                    Code: "Departments.Pagination.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetDepartmentsPagination_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            request.SearchText ??= string.Empty;

            var spec = new GetDepartmentsPaginationSpec(request);

            var (items, totalCount) = await _departmentReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var creatorApplicationUserIds = items
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<DepartmentPaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<DepartmentPaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetDepartmentCreatorsForDepartmentsPaginationSpec(creatorApplicationUserIds),
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

                    return new DepartmentPaginationItemResponse
                    {
                        Id = x.Id,
                        NameEn = x.NameEn,
                        NameAr = x.NameAr,
                        IsActive = x.IsActive,
                        CreatedBy = creator is null
                            ? null
                            : new DepartmentPaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = x.CreatedOnUtc
                    };
                })
                .ToArray();

            var response = new Pagination<DepartmentPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

            return Result<Pagination<DepartmentPaginationItemResponse>>.Ok(response);
        }
    }
}