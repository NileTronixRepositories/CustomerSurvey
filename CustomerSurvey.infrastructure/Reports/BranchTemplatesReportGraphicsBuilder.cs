using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;

namespace CustomerSurvey.infrastructure.Reports;

internal static class BranchTemplatesReportGraphicsBuilder
{
    public static BranchTemplatesReportGraphics Build(
        int totalResponses,
        IReadOnlyCollection<CalculatedResponseScore> responseScores,
        IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
        int totalAnswers,
        int includedAnswers,
        decimal? overallSatisfactionPercentage,
        decimal? averageScoreValue)
    {
        ArgumentNullException.ThrowIfNull(responseScores);
        ArgumentNullException.ThrowIfNull(questions);

        var scoredResponses = responseScores.Count(x => x.HasScore);

        return new BranchTemplatesReportGraphics
        {
            OverallSatisfactionPercentage = overallSatisfactionPercentage,
            AverageScoreValue = averageScoreValue,
            TotalResponses = totalResponses,
            ScoredResponses = scoredResponses,
            NotScoredResponses = Math.Max(0, totalResponses - scoredResponses),
            ExcellentResponses = responseScores.Count(x => x.HasScore && x.ScorePercentage >= 80m),
            GoodResponses = responseScores.Count(x => x.HasScore && x.ScorePercentage >= 60m && x.ScorePercentage < 80m),
            AverageResponses = responseScores.Count(x => x.HasScore && x.ScorePercentage >= 40m && x.ScorePercentage < 60m),
            PoorResponses = responseScores.Count(x => x.HasScore && x.ScorePercentage >= 20m && x.ScorePercentage < 40m),
            CriticalResponses = responseScores.Count(x => x.HasScore && x.ScorePercentage < 20m),
            RootQuestions = questions.Count(x => x.IsRootQuestion),
            ConditionalQuestions = questions.Count(x => !x.IsRootQuestion),
            IncludedAnswers = includedAnswers,
            NonScoredAnswers = Math.Max(0, totalAnswers - includedAnswers)
        };
    }
}
