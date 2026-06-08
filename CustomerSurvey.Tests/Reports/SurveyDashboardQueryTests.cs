using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class SurveyDashboardQueryTests
{
    [Fact]
    public void Query_DefaultScoreCalculationMode_IsRootQuestions()
    {
        var query = new GetSurveyDashboardQuery();

        Assert.Equal(ScoreCalculationMode.RootQuestions, query.ScoreCalculationMode);
    }

    [Fact]
    public void Validator_AllowsLowestConditionLevel()
    {
        var validator = new GetSurveyDashboardQueryValidator();

        var result = validator.Validate(new GetSurveyDashboardQuery
        {
            ScoreCalculationMode = ScoreCalculationMode.LowestConditionLevel
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_RejectsInvalidScoreCalculationMode()
    {
        var validator = new GetSurveyDashboardQueryValidator();

        var result = validator.Validate(new GetSurveyDashboardQuery
        {
            ScoreCalculationMode = (ScoreCalculationMode)999
        });

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetSurveyDashboardQuery.ScoreCalculationMode));
    }
}
