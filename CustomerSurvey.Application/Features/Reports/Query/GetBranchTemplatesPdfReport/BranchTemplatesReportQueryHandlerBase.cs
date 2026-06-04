using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal abstract class BranchTemplatesReportQueryHandlerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;

    protected BranchTemplatesReportQueryHandlerBase(
        ICurrentUser currentUser,
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<BranchUser> branchUserReadRepository)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAdminReadRepository = branchAdminReadRepository ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
        _branchUserReadRepository = branchUserReadRepository ?? throw new ArgumentNullException(nameof(branchUserReadRepository));
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

        var actor = await ResolveBranchActorAsync(
            _currentUser.UserId.Value,
            cancellationToken);

        if (actor is null)
        {
            return Result<BranchTemplatesPdfReportRequest>.Fail(new Error(
                Code: "Reports.TemplatesPdf.CurrentActorNotFound",
                Message: ErrorMessage.GetBranchTemplatesPdfReport_CurrentActor_NotFound,
                Type: ErrorType.NotFound));
        }

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
                Language = request.Language.ToLowerInvariant()
            });
    }

    private async Task<CurrentBranchReportActorDto?> ResolveBranchActorAsync(
        Guid applicationUserId,
        CancellationToken cancellationToken)
    {
        var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchAdminForTemplatesPdfReportSpec(applicationUserId),
            cancellationToken);

        if (branchAdmin is not null)
        {
            return branchAdmin;
        }

        var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
            new GetCurrentBranchUserForTemplatesPdfReportSpec(applicationUserId),
            cancellationToken);

        return branchUser;
    }
}
