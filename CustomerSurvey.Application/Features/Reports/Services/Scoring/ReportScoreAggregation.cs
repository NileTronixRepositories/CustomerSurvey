namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

public static class ReportScoreAggregation
{
    public static decimal? ResponseWeightedPercentage(IEnumerable<QuestionScoreToken> tokens)
    {
        var percentages = tokens.GroupBy(x => (x.TemplateKind, x.ResponseId))
            .Select(x => ReportScoreRounding.Round(x.Average(t => t.ScoreValue) * 20m)).ToArray();
        return percentages.Length == 0 ? null : ReportScoreRounding.Round(percentages.Average());
    }
}
