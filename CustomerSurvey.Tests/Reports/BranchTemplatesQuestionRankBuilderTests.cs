using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesQuestionRankBuilderTests
{
    [Fact]
    public void Build_FiltersQuestionsByDefaultThresholds()
    {
        var questions = CreateQuestionSet();

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 70m);
        var best = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: false,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 70m);

        Assert.Equal(
            new[] { "Critical", "Weak", "Worst Boundary" },
            worst.Select(x => x.QuestionTextEn));
        Assert.All(worst, question => Assert.True(question.SatisfactionPercentage <= 40m));
        Assert.Equal(
            new[] { "Excellent", "Best Boundary" },
            best.Select(x => x.QuestionTextEn));
        Assert.All(best, question => Assert.True(question.SatisfactionPercentage >= 70m));
    }

    [Fact]
    public void Build_AppliesCustomWorstThresholdAndDefaultBestThreshold()
    {
        var questions = CreateQuestionSet();

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 50m,
            bestQuestionsMinScorePercentage: 70m);
        var best = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: false,
            worstQuestionsMaxScorePercentage: 50m,
            bestQuestionsMinScorePercentage: 70m);

        Assert.Equal(
            new[] { "Critical", "Weak", "Worst Boundary", "Over Worst", "Custom Worst Boundary" },
            worst.Select(x => x.QuestionTextEn));
        Assert.All(worst, question => Assert.True(question.SatisfactionPercentage <= 50m));
        Assert.Equal(
            new[] { "Excellent", "Best Boundary" },
            best.Select(x => x.QuestionTextEn));
    }

    [Fact]
    public void Build_AppliesDefaultWorstThresholdAndCustomBestThreshold()
    {
        var questions = CreateQuestionSet();

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 60m);
        var best = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: false,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 60m);

        Assert.Equal(
            new[] { "Critical", "Weak", "Worst Boundary" },
            worst.Select(x => x.QuestionTextEn));
        Assert.Equal(
            new[] { "Excellent", "Best Boundary", "Near Best", "Custom Best Boundary" },
            best.Select(x => x.QuestionTextEn));
        Assert.All(best, question => Assert.True(question.SatisfactionPercentage >= 60m));
    }

    [Fact]
    public void Build_AppliesCustomWorstAndBestThresholds()
    {
        var questions = CreateQuestionSet();

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 50m,
            bestQuestionsMinScorePercentage: 60m);
        var best = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: false,
            worstQuestionsMaxScorePercentage: 50m,
            bestQuestionsMinScorePercentage: 60m);

        Assert.Equal(
            new[] { "Critical", "Weak", "Worst Boundary", "Over Worst", "Custom Worst Boundary" },
            worst.Select(x => x.QuestionTextEn));
        Assert.Equal(
            new[] { "Excellent", "Best Boundary", "Near Best", "Custom Best Boundary" },
            best.Select(x => x.QuestionTextEn));
    }

    [Fact]
    public void Build_OrdersBySatisfactionThenAnswerCountThenQuestionText()
    {
        var questions = new[]
        {
            CreateQuestion("Alpha", satisfactionPercentage: 40m, totalAnswers: 2),
            CreateQuestion("Gamma", satisfactionPercentage: 40m, totalAnswers: 5),
            CreateQuestion("Beta", satisfactionPercentage: 40m, totalAnswers: 5)
        };

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 70m);

        Assert.Equal(
            new[] { "Beta", "Gamma", "Alpha" },
            worst.Select(x => x.QuestionTextEn));
        Assert.Equal(new[] { 1, 2, 3 }, worst.Select(x => x.Rank));
    }

    [Fact]
    public void Build_ReturnsEmptyCollectionWhenNoQuestionsMatchThreshold()
    {
        var questions = new[]
        {
            CreateQuestion("Accepted", satisfactionPercentage: 65m)
        };

        var worst = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: true,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 70m);
        var best = BranchTemplatesQuestionRankBuilder.Build(
            questions,
            count: 10,
            worst: false,
            worstQuestionsMaxScorePercentage: 40m,
            bestQuestionsMinScorePercentage: 70m);

        Assert.Empty(worst);
        Assert.Empty(best);
    }

    private static IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> CreateQuestionSet()
        => new[]
        {
            CreateQuestion("Critical", satisfactionPercentage: 10m),
            CreateQuestion("Weak", satisfactionPercentage: 35m),
            CreateQuestion("Worst Boundary", satisfactionPercentage: 40m),
            CreateQuestion("Over Worst", satisfactionPercentage: 41m),
            CreateQuestion("Custom Worst Boundary", satisfactionPercentage: 50m),
            CreateQuestion("Custom Best Boundary", satisfactionPercentage: 60m),
            CreateQuestion("Near Best", satisfactionPercentage: 69m),
            CreateQuestion("Best Boundary", satisfactionPercentage: 70m),
            CreateQuestion("Excellent", satisfactionPercentage: 95m),
            CreateQuestion("Not Included", satisfactionPercentage: 10m, isScoreIncluded: false),
            CreateQuestion("No Score", satisfactionPercentage: null)
        };

    private static BranchTemplatesPdfQuestionAnalytics CreateQuestion(
        string text,
        decimal? satisfactionPercentage,
        int totalAnswers = 1,
        bool isScoreIncluded = true)
        => new()
        {
            TemplateId = Guid.NewGuid(),
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Template",
            TemplateQuestionId = Guid.NewGuid(),
            QuestionTextEn = text,
            IsRootQuestion = true,
            QuestionType = "StarRating",
            TotalAnswers = totalAnswers,
            IsScoreIncluded = isScoreIncluded,
            ScoreAverageValue = satisfactionPercentage.HasValue
                ? satisfactionPercentage.Value * 5m / 100m
                : null
        };
}
