using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal interface ISurveyDashboardRequestResolver
{
    Task<Result<ResolvedSurveyDashboardRequest>> ResolveAsync(
        ISurveyDashboardFilterRequest request,
        CancellationToken cancellationToken);
}

internal sealed class SurveyDashboardRequestResolver : ISurveyDashboardRequestResolver
{
    private const int DefaultPeriodDays = 30;

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public SurveyDashboardRequestResolver(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository;
        _branchReadRepository = branchReadRepository;
        _templateReadRepository = templateReadRepository;
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository;
        _currentBranchScopeResolver = currentBranchScopeResolver;
        _currentUser = currentUser;
    }

    public async Task<Result<ResolvedSurveyDashboardRequest>> ResolveAsync(
        ISurveyDashboardFilterRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Fail("Reports.SurveyDashboard.Unauthenticated",
                ErrorMessage.GetSurveyDashboard_Unauthenticated,
                ErrorType.Security);
        }

        if (request.TemplateId.HasValue && request.AnonymousTemplateId.HasValue)
        {
            return Fail("Reports.SurveyDashboard.TemplateFilterAmbiguous",
                ErrorMessage.SurveyDashboard_TemplateFilter_Ambiguous,
                ErrorType.Validation);
        }

        var actorResult = await ResolveActorAsync(_currentUser.UserId.Value, cancellationToken);
        if (actorResult.IsFailure)
        {
            return Result<ResolvedSurveyDashboardRequest>.Fail(actorResult.Errors);
        }

        var actor = actorResult.Value;
        var scopeResult = await ResolveScopeAsync(request.BranchId, actor, cancellationToken);
        if (scopeResult.IsFailure)
        {
            return Result<ResolvedSurveyDashboardRequest>.Fail(scopeResult.Errors);
        }

        var periodResult = ResolvePeriod(request.From, request.To);
        if (periodResult.IsFailure)
        {
            return Result<ResolvedSurveyDashboardRequest>.Fail(periodResult.Errors);
        }

        var templateResult = await ResolveTemplateAsync(request, scopeResult.Value, cancellationToken);
        if (templateResult.IsFailure)
        {
            return Result<ResolvedSurveyDashboardRequest>.Fail(templateResult.Errors);
        }

        var templateFilter = templateResult.Value;
        var appliedSource = templateFilter?.DashboardSource ?? request.Source;

        return Result<ResolvedSurveyDashboardRequest>.Ok(new ResolvedSurveyDashboardRequest(
            actor,
            scopeResult.Value,
            periodResult.Value,
            templateFilter,
            appliedSource,
            appliedSource is SurveyDashboardSource.All or SurveyDashboardSource.Internal,
            appliedSource is SurveyDashboardSource.All or SurveyDashboardSource.Anonymous,
            templateFilter?.TemplateKind == SurveyDashboardTemplateKind.Authorized
                ? templateFilter.TemplateId
                : null,
            templateFilter?.TemplateKind == SurveyDashboardTemplateKind.Anonymous
                ? templateFilter.TemplateId
                : null));
    }

    private async Task<Result<CurrentSurveyDashboardActor>> ResolveActorAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        if (await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == applicationUserId,
                cancellationToken))
        {
            return Result<CurrentSurveyDashboardActor>.Ok(CurrentSurveyDashboardActor.SuperAdmin());
        }

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(cancellationToken);
        if (currentBranchScope.IsFailure)
        {
            return Result<CurrentSurveyDashboardActor>.Fail(currentBranchScope.Errors);
        }

        var branch = (await _branchReadRepository.ListAsync(
            new GetSurveyDashboardBranchesSpec(currentBranchScope.Value.BranchId),
            cancellationToken)).FirstOrDefault();

        if (branch is null)
        {
            return Result<CurrentSurveyDashboardActor>.Fail(new Error(
                "Reports.SurveyDashboard.CurrentActorNotFound",
                ErrorMessage.GetSurveyDashboard_CurrentActor_NotFound,
                ErrorType.NotFound));
        }

        return Result<CurrentSurveyDashboardActor>.Ok(CurrentSurveyDashboardActor.BranchScoped(
            currentBranchScope.Value.ActorType.ToString(),
            branch));
    }

    private async Task<Result<ResolvedSurveyDashboardScope>> ResolveScopeAsync(
        Guid? requestedBranchId,
        CurrentSurveyDashboardActor actor,
        CancellationToken cancellationToken)
    {
        if (!actor.IsSuperAdmin)
        {
            if (requestedBranchId.HasValue)
            {
                return Result<ResolvedSurveyDashboardScope>.Fail(new Error(
                    "Reports.SurveyDashboard.BranchIdNotAllowed",
                    ErrorMessage.GetSurveyDashboard_BranchId_NotAllowed,
                    ErrorType.Security));
            }

            var branch = new SurveyDashboardBranchRow
            {
                BranchId = actor.BranchId!.Value,
                BranchNameEn = actor.BranchNameEn!,
                BranchNameAr = actor.BranchNameAr,
                IsActive = true
            };

            return Result<ResolvedSurveyDashboardScope>.Ok(new ResolvedSurveyDashboardScope(
                branch.BranchId,
                branch.BranchNameEn,
                branch.BranchNameAr,
                "CurrentBranch",
                new[] { branch }));
        }

        var branches = await _branchReadRepository.ListAsync(
            new GetSurveyDashboardBranchesSpec(requestedBranchId),
            cancellationToken);

        if (requestedBranchId.HasValue && branches.Count == 0)
        {
            return Result<ResolvedSurveyDashboardScope>.Fail(new Error(
                "Reports.SurveyDashboard.BranchNotFound",
                ErrorMessage.GetSurveyDashboard_Branch_NotFound,
                ErrorType.NotFound));
        }

        var selectedBranch = requestedBranchId.HasValue ? branches.First() : null;
        return Result<ResolvedSurveyDashboardScope>.Ok(new ResolvedSurveyDashboardScope(
            selectedBranch?.BranchId,
            selectedBranch?.BranchNameEn,
            selectedBranch?.BranchNameAr,
            requestedBranchId.HasValue ? "SpecificBranch" : "AllBranches",
            branches));
    }

    private async Task<Result<ResolvedSurveyDashboardTemplateFilter?>> ResolveTemplateAsync(
        ISurveyDashboardFilterRequest request,
        ResolvedSurveyDashboardScope scope,
        CancellationToken cancellationToken)
    {
        ResolvedSurveyDashboardTemplateFilter? template = null;

        if (request.TemplateId.HasValue)
        {
            template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetAuthorizedTemplateForSurveyDashboardFilterSpec(request.TemplateId.Value, scope.BranchId),
                cancellationToken);

            template ??= await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForSurveyDashboardFilterSpec(request.TemplateId.Value, scope.BranchId),
                cancellationToken);
        }
        else if (request.AnonymousTemplateId.HasValue)
        {
            template = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForSurveyDashboardFilterSpec(request.AnonymousTemplateId.Value, scope.BranchId),
                cancellationToken);
        }
        else
        {
            return Result<ResolvedSurveyDashboardTemplateFilter?>.Ok(null);
        }

        if (template is null)
        {
            return Result<ResolvedSurveyDashboardTemplateFilter?>.Fail(new Error(
                "Reports.SurveyDashboard.TemplateFilterNotFound",
                ErrorMessage.SurveyDashboard_TemplateFilter_NotFound,
                ErrorType.NotFound));
        }

        if (request.Source != SurveyDashboardSource.All && request.Source != template.DashboardSource)
        {
            return Result<ResolvedSurveyDashboardTemplateFilter?>.Fail(new Error(
                "Reports.SurveyDashboard.TemplateFilterSourceMismatch",
                ErrorMessage.SurveyDashboard_TemplateFilter_SourceMismatch,
                ErrorType.Validation));
        }

        return Result<ResolvedSurveyDashboardTemplateFilter?>.Ok(template);
    }

    private static Result<ResolvedSurveyDashboardPeriod> ResolvePeriod(DateTime? requestedFrom, DateTime? requestedTo)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var to = requestedTo.HasValue ? DateOnly.FromDateTime(requestedTo.Value) : today;
        var from = requestedFrom.HasValue ? DateOnly.FromDateTime(requestedFrom.Value) : to.AddDays(-DefaultPeriodDays);

        return from > to
            ? Result<ResolvedSurveyDashboardPeriod>.Fail(new Error(
                "Reports.SurveyDashboard.DateRangeInvalid",
                ErrorMessage.GetSurveyDashboard_DateRange_Invalid,
                ErrorType.Validation))
            : Result<ResolvedSurveyDashboardPeriod>.Ok(new ResolvedSurveyDashboardPeriod(
                from,
                to,
                !requestedFrom.HasValue && !requestedTo.HasValue));
    }

    private static Result<ResolvedSurveyDashboardRequest> Fail(string code, string message, ErrorType type)
        => Result<ResolvedSurveyDashboardRequest>.Fail(new Error(code, message, type));
}

internal sealed record ResolvedSurveyDashboardRequest(
    CurrentSurveyDashboardActor Actor,
    ResolvedSurveyDashboardScope Scope,
    ResolvedSurveyDashboardPeriod Period,
    ResolvedSurveyDashboardTemplateFilter? TemplateFilter,
    SurveyDashboardSource AppliedSource,
    bool IncludeInternal,
    bool IncludeAnonymous,
    Guid? InternalTemplateId,
    Guid? AnonymousTemplateId);

internal sealed record CurrentSurveyDashboardActor(
    bool IsSuperAdmin,
    string ActorScope,
    Guid? BranchId,
    string? BranchNameEn,
    string? BranchNameAr)
{
    public static CurrentSurveyDashboardActor SuperAdmin()
        => new(true, "SuperAdmin", null, null, null);

    public static CurrentSurveyDashboardActor BranchScoped(string actorScope, SurveyDashboardBranchRow branch)
        => new(false, actorScope, branch.BranchId, branch.BranchNameEn, branch.BranchNameAr);
}

internal sealed record ResolvedSurveyDashboardScope(
    Guid? BranchId,
    string? BranchNameEn,
    string? BranchNameAr,
    string DataScope,
    IReadOnlyCollection<SurveyDashboardBranchRow> Branches);

internal sealed record ResolvedSurveyDashboardPeriod(DateOnly From, DateOnly To, bool IsDefaultPeriod);
