using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;

internal sealed class GetBranchTemplateExcelReportQueryHandler
    : BranchTemplatesReportQueryHandlerBase,
      IQueryHandler<GetBranchTemplateExcelReportQuery, GetBranchTemplateExcelReportResponse>
{
    private readonly IBranchTemplateExcelReportService _reportService;

    public GetBranchTemplateExcelReportQueryHandler(
        ICurrentUser currentUser,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        IBranchTemplateExcelReportService reportService)
        : base(currentUser, currentBranchScopeResolver, applicationUserReadRepository)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    public async Task<Result<GetBranchTemplateExcelReportResponse>> Handle(
        GetBranchTemplateExcelReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportRequest = await BuildReportRequestAsync(request, cancellationToken);

        if (reportRequest.IsFailure)
        {
            return Result<GetBranchTemplateExcelReportResponse>.Fail(reportRequest.Errors);
        }

        var file = await _reportService.GenerateAsync(
            reportRequest.Value,
            cancellationToken);

        if (file.IsFailure)
        {
            return Result<GetBranchTemplateExcelReportResponse>.Fail(file.Errors);
        }

        return Result<GetBranchTemplateExcelReportResponse>.Ok(
            new GetBranchTemplateExcelReportResponse
            {
                FileName = file.Value.FileName,
                ContentType = file.Value.ContentType,
                Content = file.Value.Content
            });
    }
}
