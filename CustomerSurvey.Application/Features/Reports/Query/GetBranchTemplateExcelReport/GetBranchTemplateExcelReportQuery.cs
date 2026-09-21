using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;

public sealed record GetBranchTemplateExcelReportQuery
    : IQuery<GetBranchTemplateExcelReportResponse>,
      IBranchTemplatesReportQueryParameters
{
    public DateOnly FromDate { get; init; }

    public DateOnly ToDate { get; init; }

    public Guid? TemplateId { get; init; }

    public ReportTemplateKind? TemplateKind { get; init; }

    public ScoreCalculationMode ScoreCalculationMode { get; init; }
        = ScoreCalculationMode.RootQuestions;

    public int TopWorstQuestionsCount { get; init; } = 5;

    public decimal? WorstQuestionsMaxScorePercentage { get; init; }

    public decimal? BestQuestionsMinScorePercentage { get; init; }

    /// <summary>
    /// Comes from Accept-Language header inside controller.
    /// Supported values: ar, en.
    /// </summary>
    public string Language { get; init; } = "en";
}
