namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

public static class BranchTemplatesReportQuestionRankThresholds
{
    public const decimal DefaultWorstQuestionsMaxScorePercentage = 40m;

    public const decimal DefaultBestQuestionsMinScorePercentage = 70m;

    public static decimal ResolveWorstQuestionsMaxScorePercentage(decimal? value)
        => value ?? DefaultWorstQuestionsMaxScorePercentage;

    public static decimal ResolveBestQuestionsMinScorePercentage(decimal? value)
        => value ?? DefaultBestQuestionsMinScorePercentage;
}
