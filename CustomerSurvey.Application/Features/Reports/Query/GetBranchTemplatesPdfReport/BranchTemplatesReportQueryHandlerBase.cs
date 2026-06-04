using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal abstract class BranchTemplatesReportQueryHandlerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;

    protected BranchTemplatesReportQueryHandlerBase(
        ICurrentUser currentUser,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentBranchScopeResolver = currentBranchScopeResolver ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));
        _applicationUserReadRepository = applicationUserReadRepository ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));
    }

    protected async Task<Result<BranchTemplatesPdfReportRequest>> BuildReportRequestAsync(
        IBranchTemplatesReportQueryParameters request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<BranchTemplatesPdfReportRequest>.Fail(new Error(
                Code: "Reports.TemplatesPdf.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var isArabic = string.Equals(request.Language, "ar", StringComparison.OrdinalIgnoreCase);

        var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
            cancellationToken);

        if (currentBranchScope.IsFailure)
        {
            return Result<BranchTemplatesPdfReportRequest>.Fail(currentBranchScope.Errors);
        }

        var applicationUser = await _applicationUserReadRepository.FirstOrDefaultAsync(
            new GetCurrentApplicationUserForTemplatesPdfReportSpec(currentBranchScope.Value.ApplicationUserId),
            cancellationToken);

        if (applicationUser is null)
        {
            return Result<BranchTemplatesPdfReportRequest>.Fail(new Error(
                Code: "Reports.TemplatesPdf.CurrentActorNotFound",
                Message: ErrorMessage.GetBranchTemplatesPdfReport_CurrentActor_NotFound,
                Type: ErrorType.NotFound));
        }

        var actor = new CurrentBranchReportActorDto
        {
            ApplicationUserId = currentBranchScope.Value.ApplicationUserId,
            BranchId = currentBranchScope.Value.BranchId,
            NameEn = applicationUser.NameEn,
            NameAr = applicationUser.NameAr
        };

        return Result<BranchTemplatesPdfReportRequest>.Ok(
            new BranchTemplatesPdfReportRequest
            {
                BranchId = actor.BranchId,
                GeneratedByApplicationUserId = actor.ApplicationUserId,
                GeneratedByName = actor.DisplayName(isArabic),
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TemplateId = request.TemplateId,
                TemplateKind = request.TemplateKind,
                ScoreCalculationMode = request.ScoreCalculationMode,
                TopWorstQuestionsCount = request.TopWorstQuestionsCount,
                WorstQuestionsMaxScorePercentage = request.WorstQuestionsMaxScorePercentage,
                BestQuestionsMinScorePercentage = request.BestQuestionsMinScorePercentage,
                Language = request.Language.ToLowerInvariant()
            });
    }

}
