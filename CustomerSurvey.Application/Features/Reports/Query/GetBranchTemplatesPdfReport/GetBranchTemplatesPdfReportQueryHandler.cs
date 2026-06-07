using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed class GetBranchTemplatesPdfReportQueryHandler
    : BranchTemplatesReportQueryHandlerBase,
      IQueryHandler<GetBranchTemplatesPdfReportQuery, GetBranchTemplatesPdfReportResponse>
{
    private readonly IBranchTemplatesPdfReportService _reportService;

    public GetBranchTemplatesPdfReportQueryHandler(
        ICurrentUser currentUser,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
        IBranchTemplatesPdfReportService reportService)
        : base(currentUser, currentBranchScopeResolver, applicationUserReadRepository)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    public async Task<Result<GetBranchTemplatesPdfReportResponse>> Handle(
        GetBranchTemplatesPdfReportQuery request,
        CancellationToken cancellationToken)
    {
        var reportRequest = await BuildReportRequestAsync(request, cancellationToken);

        if (reportRequest.IsFailure)
        {
            return Result<GetBranchTemplatesPdfReportResponse>.Fail(reportRequest.Errors);
        }

        var file = await _reportService.GenerateAsync(
            reportRequest.Value,
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
}
