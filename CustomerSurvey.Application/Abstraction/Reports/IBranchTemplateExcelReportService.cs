using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.Application.Abstraction.Reports;

public interface IBranchTemplateExcelReportService
{
    Task<Result<BranchTemplateExcelReportFile>> GenerateAsync(
        BranchTemplatesPdfReportRequest request,
        CancellationToken cancellationToken);
}

public sealed record BranchTemplateExcelReportFile
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; }
        = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public byte[] Content { get; init; } = Array.Empty<byte>();
}
