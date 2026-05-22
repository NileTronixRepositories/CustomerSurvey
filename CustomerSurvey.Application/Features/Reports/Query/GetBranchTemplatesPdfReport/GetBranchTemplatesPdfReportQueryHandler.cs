using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport.Specs;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed class GetBranchTemplatesPdfReportQueryHandler
    : IQueryHandler<GetBranchTemplatesPdfReportQuery, GetBranchTemplatesPdfReportResponse>
{
    private readonly ICurrentUser _currentUser;
    private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
    private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
    private readonly IBranchTemplatesPdfReportService _reportService;

    public GetBranchTemplatesPdfReportQueryHandler(
        ICurrentUser currentUser,
        IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
        IWriteReadRepository<BranchUser> branchUserReadRepository,
        IBranchTemplatesPdfReportService reportService)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _branchAdminReadRepository = branchAdminReadRepository ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));
        _branchUserReadRepository = branchUserReadRepository ?? throw new ArgumentNullException(nameof(branchUserReadRepository));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    public async Task<Result<GetBranchTemplatesPdfReportResponse>> Handle(
        GetBranchTemplatesPdfReportQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetBranchTemplatesPdfReportResponse>.Fail(new Error(
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
            return Result<GetBranchTemplatesPdfReportResponse>.Fail(new Error(
                Code: "Reports.TemplatesPdf.CurrentActorNotFound",
                Message: ErrorMessage.GetBranchTemplatesPdfReport_CurrentActor_NotFound,
                Type: ErrorType.NotFound));
        }

        var file = await _reportService.GenerateAsync(
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
            },
            cancellationToken);

        if (file.IsFailure)
        {
            return Result<GetBranchTemplatesPdfReportResponse>.Fail(file.Errors);
        }

        return Result<GetBranchTemplatesPdfReportResponse>.Ok(
            new GetBranchTemplatesPdfReportResponse
            {
                FileName = file.Value.FileName,
                ContentType = file.Value.ContentType,
                Content = file.Value.Content
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
