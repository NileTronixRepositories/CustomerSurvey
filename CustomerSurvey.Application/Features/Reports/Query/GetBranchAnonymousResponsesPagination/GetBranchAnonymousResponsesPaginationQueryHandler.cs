using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;

internal sealed class GetBranchAnonymousResponsesPaginationQueryHandler
    : IQueryHandler<GetBranchAnonymousResponsesPaginationQuery, Pagination<BranchAnonymousResponsePaginationItemResponse>>
{
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchAnonymousResponsesPaginationQueryHandler(
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<BranchUser> branchUserReadRepository,
        ICurrentUser currentUser)
    {
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

        _branchAdminReadRepository = branchAdminReadRepository
            ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

        _branchUserReadRepository = branchUserReadRepository
            ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>> Handle(
        GetBranchAnonymousResponsesPaginationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.BranchAnonymousResponsesPagination.Unauthenticated",
                Message: ErrorMessage.GetBranchAnonymousResponses_Unauthenticated,
                Type: ErrorType.Security));
        }

        request.SearchText ??= string.Empty;

        var currentActor = await ResolveCurrentBranchActorAsync(
            _currentUser.UserId.Value,
            cancellationToken);

        if (currentActor is null)
        {
            return Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>.Fail(new Error(
                Code: "Reports.BranchAnonymousResponsesPagination.CurrentBranchActorNotFound",
                Message: ErrorMessage.GetBranchAnonymousResponses_CurrentBranchActor_NotFound,
                Type: ErrorType.NotFound));
        }

        if (request.AnonymousTemplateId.HasValue)
        {
            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForBranchAnonymousResponsesPaginationSpec(
                    request.AnonymousTemplateId.Value,
                    currentActor.BranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>.Fail(new Error(
                    Code: "Reports.BranchAnonymousResponsesPagination.AnonymousTemplateNotFound",
                    Message: ErrorMessage.GetBranchAnonymousResponses_AnonymousTemplate_NotFound,
                    Type: ErrorType.NotFound));
            }
        }

        var period = ResolvePeriod(request);

        var spec = new GetBranchAnonymousResponsesPaginationSpec(
            currentActor.BranchId,
            period.FromUtc,
            period.ToExclusiveUtc,
            request);

        var (items, totalCount) = await _anonymousSurveyResponseReadRepository.ListWithCountAsync(
            spec,
            cancellationToken);

        var response = new Pagination<BranchAnonymousResponsePaginationItemResponse>(
            currentPage: request.PageNumber,
            pageSize: request.PageSize,
            totalItems: totalCount,
            data: items);

        return Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>.Ok(response);
    }

    private async Task<CurrentBranchActorForBranchAnonymousResponsesPaginationDto?> ResolveCurrentBranchActorAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchAdminForBranchAnonymousResponsesPaginationSpec(applicationUserId),
            cancellationToken);

        if (branchAdmin is not null)
        {
            return branchAdmin;
        }

        return await _branchUserReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchUserForBranchAnonymousResponsesPaginationSpec(applicationUserId),
            cancellationToken);
    }

    private static ResolvedPaginationPeriod ResolvePeriod(
        GetBranchAnonymousResponsesPaginationQuery request)
    {
        var fromUtc = request.From?.Date;
        var toExclusiveUtc = request.To?.Date.AddDays(1);

        return new ResolvedPaginationPeriod(
            FromUtc: fromUtc,
            ToExclusiveUtc: toExclusiveUtc);
    }

    private sealed record ResolvedPaginationPeriod(
        DateTime? FromUtc,
        DateTime? ToExclusiveUtc);

    private sealed record CurrentBranchActorForBranchAnonymousResponsesPaginationDto
    {
        public Guid BranchId { get; init; }
    }

    private sealed record AnonymousTemplateForBranchAnonymousResponsesPaginationDto
    {
        public Guid AnonymousTemplateId { get; init; }
    }

    private sealed class GetCurrentBranchAdminForBranchAnonymousResponsesPaginationSpec
        : Specification<BranchAdmin, CurrentBranchActorForBranchAnonymousResponsesPaginationDto>
    {
        public GetCurrentBranchAdminForBranchAnonymousResponsesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForBranchAnonymousResponsesPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }

    private sealed class GetCurrentBranchUserForBranchAnonymousResponsesPaginationSpec
        : Specification<BranchUser, CurrentBranchActorForBranchAnonymousResponsesPaginationDto>
    {
        public GetCurrentBranchUserForBranchAnonymousResponsesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForBranchAnonymousResponsesPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }

    private sealed class GetAnonymousTemplateForBranchAnonymousResponsesPaginationSpec
        : Specification<AnonymousTemplate, AnonymousTemplateForBranchAnonymousResponsesPaginationDto>
    {
        public GetAnonymousTemplateForBranchAnonymousResponsesPaginationSpec(
            Guid anonymousTemplateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == anonymousTemplateId &&
                x.Scope == AnonymousTemplateScope.Branch &&
                x.BranchId == branchId);

            Select(x => new AnonymousTemplateForBranchAnonymousResponsesPaginationDto
            {
                AnonymousTemplateId = x.Id
            });
        }
    }
}
