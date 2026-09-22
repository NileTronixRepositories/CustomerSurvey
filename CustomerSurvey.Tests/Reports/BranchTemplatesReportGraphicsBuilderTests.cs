using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesReportGraphicsBuilderTests
{
    [Fact]
    public void Build_UsesExpectedDistributionBoundariesAndExcludesUnscoredResponses()
    {
        var scores = new[]
        {
            Score(80m),
            Score(60m),
            Score(40m),
            Score(20m),
            Score(19.99m)
        };
        var questions = new[]
        {
            new BranchTemplatesPdfQuestionAnalytics { IsRootQuestion = true },
            new BranchTemplatesPdfQuestionAnalytics { IsRootQuestion = false }
        };

        var result = BranchTemplatesReportGraphicsBuilder.Build(
            totalResponses: 7,
            responseScores: scores,
            questions,
            totalAnswers: 12,
            includedAnswers: 5,
            overallSatisfactionPercentage: 55m,
            averageScoreValue: 2.75m);

        Assert.Equal(55m, result.OverallSatisfactionPercentage);
        Assert.Equal(2.75m, result.AverageScoreValue);
        Assert.Equal(5, result.ScoredResponses);
        Assert.Equal(2, result.NotScoredResponses);
        Assert.Equal(1, result.ExcellentResponses);
        Assert.Equal(1, result.GoodResponses);
        Assert.Equal(1, result.AverageResponses);
        Assert.Equal(1, result.PoorResponses);
        Assert.Equal(1, result.CriticalResponses);
        Assert.Equal(1, result.RootQuestions);
        Assert.Equal(1, result.ConditionalQuestions);
        Assert.Equal(5, result.IncludedAnswers);
        Assert.Equal(7, result.NonScoredAnswers);
    }

    [Fact]
    public void Build_EmptyReportPreservesNoScoreState()
    {
        var result = BranchTemplatesReportGraphicsBuilder.Build(
            totalResponses: 0,
            responseScores: Array.Empty<CalculatedResponseScore>(),
            questions: Array.Empty<BranchTemplatesPdfQuestionAnalytics>(),
            totalAnswers: 0,
            includedAnswers: 0,
            overallSatisfactionPercentage: null,
            averageScoreValue: null);

        Assert.Null(result.OverallSatisfactionPercentage);
        Assert.Null(result.AverageScoreValue);
        Assert.Equal(0, result.ScoreDistributionTotal);
        Assert.Equal(0, result.CriticalResponses);
    }

    private static CalculatedResponseScore Score(decimal percentage)
        => new()
        {
            ResponseId = Guid.NewGuid(),
            TemplateId = Guid.NewGuid(),
            TemplateKind = ReportTemplateKind.Normal,
            ScoredItemsCount = 1,
            AverageScoreValue = percentage * 5m / 100m,
            ScorePercentage = percentage
        };
}
