using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.Application.Abstraction.Reports;

public interface IBranchTemplatesPdfReportService
{
    Task<Result<BranchTemplatesPdfReportModel>> BuildReportModelAsync(
        BranchTemplatesPdfReportRequest request,
        CancellationToken cancellationToken);

    Task<Result<BranchTemplatesPdfReportFile>> GenerateAsync(
        BranchTemplatesPdfReportRequest request,
        CancellationToken cancellationToken);
}

public sealed record BranchTemplatesPdfReportRequest
{
    public Guid BranchId { get; init; }

    public Guid GeneratedByApplicationUserId { get; init; }

    public string GeneratedByName { get; init; } = string.Empty;

    public DateOnly FromDate { get; init; }

    public DateOnly ToDate { get; init; }

    public Guid? TemplateId { get; init; }

    public ReportTemplateKind? TemplateKind { get; init; }

    public ScoreCalculationMode ScoreCalculationMode { get; init; }
        = ScoreCalculationMode.RootQuestions;

    public int TopWorstQuestionsCount { get; init; } = 5;

    public decimal? WorstQuestionsMaxScorePercentage { get; init; }

    public decimal? BestQuestionsMinScorePercentage { get; init; }

    public string Language { get; init; } = "en";
}

public sealed record BranchTemplatesPdfReportFile
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/pdf";

    public byte[] Content { get; init; } = Array.Empty<byte>();
}
