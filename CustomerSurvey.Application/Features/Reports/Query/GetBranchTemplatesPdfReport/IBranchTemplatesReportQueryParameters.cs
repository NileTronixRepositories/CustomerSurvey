namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

public interface IBranchTemplatesReportQueryParameters
{
    DateOnly FromDate { get; }

    DateOnly ToDate { get; }

    Guid? TemplateId { get; }

    ReportTemplateKind? TemplateKind { get; }

    ScoreCalculationMode ScoreCalculationMode { get; }

    int TopWorstQuestionsCount { get; }

    string Language { get; }
}
