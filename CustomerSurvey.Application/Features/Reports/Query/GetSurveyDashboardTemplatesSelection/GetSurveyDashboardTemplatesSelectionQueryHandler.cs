using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System.Globalization;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;

internal sealed class GetSurveyDashboardTemplatesSelectionQueryHandler
    : IQueryHandler<GetSurveyDashboardTemplatesSelectionQuery, IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>
{
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly IWriteReadRepository<Branch> _branchReadRepository;
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly ICurrentUser _currentUser;

    public GetSurveyDashboardTemplatesSelectionQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        IWriteReadRepository<Branch> branchReadRepository,
        IWriteReadRepository<Template> templateReadRepository,
        IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));
        _branchReadRepository = branchReadRepository
            ?? throw new ArgumentNullException(nameof(branchReadRepository));
        _templateReadRepository = templateReadRepository
            ?? throw new ArgumentNullException(nameof(templateReadRepository));
        _anonymousTemplateReadRepository = anonymousTemplateReadRepository
            ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));
        _currentBranchScopeResolver = currentBranchScopeResolver
            ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>> Handle(
        GetSurveyDashboardTemplatesSelectionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>.Fail(new Error(
                Code: "Reports.SurveyDashboardTemplatesSelection.Unauthenticated",
                Message: ErrorMessage.GetSurveyDashboard_Unauthenticated,
                Type: ErrorType.Security));
        }

        var branchScopeResult = await ResolveBranchScopeAsync(
            request,
            _currentUser.UserId.Value,
            cancellationToken);

        if (branchScopeResult.IsFailure)
        {
            return Result<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>.Fail(
                branchScopeResult.Errors);
        }

        var branchExists = await _branchReadRepository.FirstOrDefaultAsync(
            new GetSurveyDashboardTemplatesSelectionBranchSpec(branchScopeResult.Value),
            cancellationToken);

        if (branchExists is null)
        {
            return Result<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>.Fail(new Error(
                Code: "Reports.SurveyDashboardTemplatesSelection.CurrentBranchScopeNotFound",
                Message: ErrorMessage.SurveyDashboardTemplatesSelection_CurrentBranchScope_NotFound,
                Type: ErrorType.NotFound));
        }

        var useArabicDisplayName = string.Equals(
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "ar",
            StringComparison.OrdinalIgnoreCase);

        var includeAuthorized = request.TemplateKind is null or SurveyDashboardTemplateKind.Authorized;
        var includeAnonymous = request.TemplateKind is null or SurveyDashboardTemplateKind.Anonymous;

        var items = new List<SurveyDashboardTemplateSelectionResponse>();

        if (includeAuthorized)
        {
            items.AddRange(await _templateReadRepository.ListAsync(
                new GetAuthorizedTemplatesForSurveyDashboardSelectionSpec(
                    branchScopeResult.Value,
                    request.SearchText,
                    useArabicDisplayName),
                cancellationToken));
        }

        if (includeAnonymous)
        {
            items.AddRange(await _anonymousTemplateReadRepository.ListAsync(
                new GetAnonymousTemplatesForSurveyDashboardSelectionSpec(
                    branchScopeResult.Value,
                    request.SearchText,
                    useArabicDisplayName),
                cancellationToken));
        }

        return Result<IReadOnlyCollection<SurveyDashboardTemplateSelectionResponse>>.Ok(
            items
                .OrderBy(x => x.DisplayName)
                .ThenBy(x => x.TemplateKind)
                .ThenBy(x => x.NameEn)
                .ToArray());
    }

    private async Task<Result<Guid>> ResolveBranchScopeAsync(
        GetSurveyDashboardTemplatesSelectionQuery request,
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == applicationUserId,
            cancellationToken);

        if (isSuperAdmin)
        {
            if (!request.BranchId.HasValue)
            {
                return Result<Guid>.Fail(new Error(
                    Code: "Reports.SurveyDashboardTemplatesSelection.BranchIdRequiredForSuperAdmin",
                    Message: ErrorMessage.SurveyDashboardTemplatesSelection_BranchId_Required_ForSuperAdmin,
                    Type: ErrorType.Validation));
            }

            return Result<Guid>.Ok(request.BranchId.Value);
        }

        if (request.BranchId.HasValue)
        {
            return Result<Guid>.Fail(new Error(
                Code: "Reports.SurveyDashboardTemplatesSelection.BranchIdNotAllowed",
                Message: ErrorMessage.GetSurveyDashboard_BranchId_NotAllowed,
                Type: ErrorType.Security));
        }

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
            cancellationToken);

        if (currentBranchScope.IsFailure)
        {
            return Result<Guid>.Fail(currentBranchScope.Errors);
        }

        return Result<Guid>.Ok(currentBranchScope.Value.BranchId);
    }
}
