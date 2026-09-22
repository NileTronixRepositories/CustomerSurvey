using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed class GetSuperAdminTemplatesPaginationQueryHandler
    : IQueryHandler<GetSuperAdminTemplatesPaginationQuery, Pagination<SuperAdminTemplatePaginationItemResponse>>
{
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetSuperAdminTemplatesPaginationQueryHandler(
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        ICurrentUser currentUser)
    {
        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));

        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));

        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

        _applicationUserReadRepository = applicationUserReadRepository
            ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<SuperAdminTemplatePaginationItemResponse>>> Handle(
        GetSuperAdminTemplatesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<SuperAdminTemplatePaginationItemResponse>>.Fail(new Error(
                Code: "Templates.SuperAdminPagination.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentApplicationUserId = _currentUser.UserId.Value;

        var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == currentApplicationUserId,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            return Result<Pagination<SuperAdminTemplatePaginationItemResponse>>.Fail(new Error(
                Code: "Templates.SuperAdminPagination.CurrentSuperAdminNotFound",
                Message: ErrorMessage.GetSuperAdminTemplatesPagination_CurrentSuperAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        request.SearchText ??= string.Empty;

        if (request.BranchId.HasValue)
        {
            var branchExists = await _branchReadRepository.AnyAsync(
                x => x.Id == request.BranchId.Value,
                cancellationToken);

            if (!branchExists)
            {
                return Result<Pagination<SuperAdminTemplatePaginationItemResponse>>.Fail(new Error(
                    Code: "Templates.SuperAdminPagination.BranchNotFound",
                    Message: ErrorMessage.GetSuperAdminTemplatesPagination_Branch_NotFound,
                    Type: ErrorType.NotFound));
            }
        }

        var templateKind = request.TemplateKind ?? TemplateCatalogKind.AuthorizeAndAnonymous;

        var (items, totalCount) = templateKind switch
        {
            TemplateCatalogKind.Authorized => await GetAuthorizedTemplatesPageAsync(
                request,
                cancellationToken),

            TemplateCatalogKind.Anonymous => await GetAnonymousTemplatesPageAsync(
                request,
                cancellationToken),

            _ => await GetAllTemplatesPageAsync(
                request,
                cancellationToken)
        };

        var responseItems = await MapResponseItemsAsync(
            items,
            cancellationToken);

        var response = new Pagination<SuperAdminTemplatePaginationItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: responseItems);

        return Result<Pagination<SuperAdminTemplatePaginationItemResponse>>.Ok(response);
    }

    private async Task<(IReadOnlyCollection<SuperAdminTemplatePaginationItemDto> Items, int TotalCount)> GetAuthorizedTemplatesPageAsync(
        GetSuperAdminTemplatesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _templateReadRepository.ListWithCountAsync(
            new GetSuperAdminAuthorizedTemplatesPaginationSpec(request),
            cancellationToken);

        return (items, totalCount);
    }

    private async Task<(IReadOnlyCollection<SuperAdminTemplatePaginationItemDto> Items, int TotalCount)> GetAnonymousTemplatesPageAsync(
        GetSuperAdminTemplatesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _anonymousTemplateReadRepository.ListWithCountAsync(
            new GetSuperAdminAnonymousTemplatesPaginationSpec(request),
            cancellationToken);

        return (items, totalCount);
    }

    private async Task<(IReadOnlyCollection<SuperAdminTemplatePaginationItemDto> Items, int TotalCount)> GetAllTemplatesPageAsync(
        GetSuperAdminTemplatesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var itemsToReadPerKind = checked(((request.PageNumber - 1) * request.PageSize) + request.PageSize);
        var firstItemsRequest = CreateFirstItemsRequest(
            request,
            itemsToReadPerKind);

        var (authorizedItems, authorizedTotalCount) = await _templateReadRepository.ListWithCountAsync(
            new GetSuperAdminAuthorizedTemplatesPaginationSpec(firstItemsRequest),
            cancellationToken);

        var (anonymousItems, anonymousTotalCount) = await _anonymousTemplateReadRepository.ListWithCountAsync(
            new GetSuperAdminAnonymousTemplatesPaginationSpec(firstItemsRequest),
            cancellationToken);

        var skip = (request.PageNumber - 1) * request.PageSize;

        var items = SortItems(
                authorizedItems.Concat(anonymousItems),
                request.OrderSort)
            .Skip(skip)
            .Take(request.PageSize)
            .ToArray();

        return (
            items,
            authorizedTotalCount + anonymousTotalCount);
    }

    private static GetSuperAdminTemplatesPaginationQuery CreateFirstItemsRequest(
        GetSuperAdminTemplatesPaginationQuery request,
        int pageSize)
    {
        return new GetSuperAdminTemplatesPaginationQuery
        {
            BranchId = request.BranchId,
            TemplateKind = request.TemplateKind,
            IsActive = request.IsActive,
            SearchText = request.SearchText,
            OrderSort = request.OrderSort,
            PageNumber = 1,
            PageSize = pageSize
        };
    }

    private static IOrderedEnumerable<SuperAdminTemplatePaginationItemDto> SortItems(
        IEnumerable<SuperAdminTemplatePaginationItemDto> items,
        OrderSort orderSort)
    {
        return orderSort == OrderSort.Oldest
            ? items
                .OrderBy(x => x.CreatedOnUtc)
                .ThenBy(x => x.TemplateKind)
                .ThenBy(x => x.TemplateId)
            : items
                .OrderByDescending(x => x.CreatedOnUtc)
                .ThenBy(x => x.TemplateKind)
                .ThenBy(x => x.TemplateId);
    }

    private async Task<IReadOnlyList<SuperAdminTemplatePaginationItemResponse>> MapResponseItemsAsync(
        IReadOnlyCollection<SuperAdminTemplatePaginationItemDto> items,
        CancellationToken cancellationToken)
    {
        var creatorApplicationUserIds = items
            .Select(x => x.CreatedByApplicationUserId)
            .Distinct()
            .ToArray();

        IReadOnlyCollection<SuperAdminTemplatePaginationCreatorDto> creators;

        if (creatorApplicationUserIds.Length == 0)
        {
            creators = Array.Empty<SuperAdminTemplatePaginationCreatorDto>();
        }
        else
        {
            creators = await _applicationUserReadRepository.ListAsync(
                new GetSuperAdminTemplateCreatorsForPaginationSpec(creatorApplicationUserIds),
                cancellationToken);
        }

        var creatorsByApplicationUserId = creators.ToDictionary(
            x => x.ApplicationUserId,
            x => x);

        return items
            .Select(x =>
            {
                creatorsByApplicationUserId.TryGetValue(
                    x.CreatedByApplicationUserId,
                    out var creator);

                return new SuperAdminTemplatePaginationItemResponse
                {
                    TemplateId = x.TemplateId,
                    BranchId = x.BranchId,
                    BranchNameEn = x.BranchNameEn,
                    BranchNameAr = x.BranchNameAr,
                    TemplateKind = x.TemplateKind,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    LogoPath = x.LogoPath,
                    QuestionsCount = x.QuestionsCount,
                    CustomInputsCount = x.CustomInputsCount,
                    PublicUrl = x.PublicUrl,
                    QrCode = x.QrCode,
                    CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                    CreatedBy = creator is null
                        ? null
                        : new SuperAdminTemplatePaginationCreatedByResponse
                        {
                            NameEn = creator.NameEn,
                            NameAr = creator.NameAr
                        },
                    CreatedOnUtc = x.CreatedOnUtc,
                    ActiveFrom = x.ActiveFrom,
                    ExpireTo = x.ExpireTo
                };
            })
            .ToArray();
    }
}
