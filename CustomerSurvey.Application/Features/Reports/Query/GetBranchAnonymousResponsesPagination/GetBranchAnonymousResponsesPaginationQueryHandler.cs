using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
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
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public GetBranchAnonymousResponsesPaginationQueryHandler(
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser)
    {
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

        _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

        _currentBranchScopeResolver = currentBranchScopeResolver
            ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

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

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
            cancellationToken);

        if (currentBranchScope.IsFailure)
        {
            return Result<Pagination<BranchAnonymousResponsePaginationItemResponse>>.Fail(
                currentBranchScope.Errors);
        }

        var branchId = currentBranchScope.Value.BranchId;

        if (request.AnonymousTemplateId.HasValue)
        {
            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForBranchAnonymousResponsesPaginationSpec(
                    request.AnonymousTemplateId.Value,
                    branchId),
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
            branchId,
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

    private sealed record AnonymousTemplateForBranchAnonymousResponsesPaginationDto
    {
        public Guid AnonymousTemplateId { get; init; }
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
