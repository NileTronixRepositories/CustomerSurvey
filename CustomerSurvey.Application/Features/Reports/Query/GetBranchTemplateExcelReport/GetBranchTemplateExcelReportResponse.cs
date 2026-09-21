namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;

public sealed record GetBranchTemplateExcelReportResponse
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; }
        = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public byte[] Content { get; init; } = Array.Empty<byte>();
}
