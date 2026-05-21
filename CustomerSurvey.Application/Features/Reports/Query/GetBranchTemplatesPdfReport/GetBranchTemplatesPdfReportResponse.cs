namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

public sealed record GetBranchTemplatesPdfReportResponse
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/pdf";

    public byte[] Content { get; init; } = Array.Empty<byte>();
}