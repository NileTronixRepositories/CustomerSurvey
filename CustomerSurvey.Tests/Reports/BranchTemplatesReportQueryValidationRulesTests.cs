using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Resources;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesReportQueryValidationRulesTests
{
    private readonly GetBranchTemplatesReportQueryValidator _validator = new();

    [Fact]
    public void Validate_AllowsMissingThresholdsAndDefaultsResolveToBusinessValues()
    {
        var result = _validator.Validate(CreateQuery());

        Assert.True(result.IsValid);
        Assert.Equal(
            40m,
            BranchTemplatesReportQuestionRankThresholds.ResolveWorstQuestionsMaxScorePercentage(null));
        Assert.Equal(
            70m,
            BranchTemplatesReportQuestionRankThresholds.ResolveBestQuestionsMinScorePercentage(null));
    }

    [Theory]
    [MemberData(nameof(ValidThresholdCases))]
    public void Validate_AllowsValidThresholdCombinations(decimal? worst, decimal? best)
    {
        var result = _validator.Validate(CreateQuery(worst, best));

        Assert.True(result.IsValid);
    }

    [Theory]
    [MemberData(nameof(InvalidPercentageCases))]
    public void Validate_RejectsThresholdsOutsidePercentageRange(
        decimal? worst,
        decimal? best,
        string propertyName)
    {
        var result = _validator.Validate(CreateQuery(worst, best));

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == propertyName);
    }

    [Fact]
    public void Validate_RejectsWorstThresholdGreaterThanBestThresholdAfterDefaults()
    {
        var result = _validator.Validate(CreateQuery(worst: 80m, best: 60m));

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage ==
                ErrorMessage.GetBranchTemplatesPdfReport_QuestionRankThresholds_Invalid);
    }

    public static TheoryData<decimal?, decimal?> ValidThresholdCases()
        => new()
        {
            { null, null },
            { 50m, null },
            { null, 60m },
            { 50m, 60m },
            { 0m, 100m },
            { 70m, 70m }
        };

    public static TheoryData<decimal?, decimal?, string> InvalidPercentageCases()
        => new()
        {
            { -1m, null, nameof(GetBranchTemplatesReportQuery.WorstQuestionsMaxScorePercentage) },
            { 101m, null, nameof(GetBranchTemplatesReportQuery.WorstQuestionsMaxScorePercentage) },
            { null, -1m, nameof(GetBranchTemplatesReportQuery.BestQuestionsMinScorePercentage) },
            { null, 101m, nameof(GetBranchTemplatesReportQuery.BestQuestionsMinScorePercentage) }
        };

    private static GetBranchTemplatesReportQuery CreateQuery(
        decimal? worst = null,
        decimal? best = null)
        => new()
        {
            FromDate = new DateOnly(2026, 4, 22),
            ToDate = new DateOnly(2026, 5, 22),
            TopWorstQuestionsCount = 10,
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            Language = "en",
            WorstQuestionsMaxScorePercentage = worst,
            BestQuestionsMinScorePercentage = best
        };
}
