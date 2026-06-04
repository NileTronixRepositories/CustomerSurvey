using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed class GetBranchTemplatesReportQueryHandler
    : BranchTemplatesReportQueryHandlerBase,
      IQueryHandler<GetBranchTemplatesReportQuery, BranchTemplatesPdfReportModel>
{
    private readonly IBranchTemplatesPdfReportService _reportService;

    public GetBranchTemplatesReportQueryHandler(
        ICurrentUser currentUser,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        IBranchTemplatesPdfReportService reportService)
        : base(currentUser, currentBranchScopeResolver, applicationUserReadRepository)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    public async Task<Result<BranchTemplatesPdfReportModel>> Handle(
        GetBranchTemplatesReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportRequest = await BuildReportRequestAsync(request, cancellationToken);

        if (reportRequest.IsFailure)
        {
            return Result<BranchTemplatesPdfReportModel>.Fail(reportRequest.Errors);
        }

        return await _reportService.BuildReportModelAsync(
            reportRequest.Value,
            cancellationToken);
    }
}
