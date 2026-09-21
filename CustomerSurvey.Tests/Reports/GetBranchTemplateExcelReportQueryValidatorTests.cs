using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class GetBranchTemplateExcelReportQueryValidatorTests
{
    private readonly GetBranchTemplateExcelReportQueryValidator _validator = new();

    [Fact]
    public void Validate_RejectsMissingTemplateId()
    {
        var result = _validator.Validate(CreateQuery(templateId: null));

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetBranchTemplateExcelReportQuery.TemplateId));
    }

    [Fact]
    public void Validate_RejectsEmptyTemplateId()
    {
        var result = _validator.Validate(CreateQuery(Guid.Empty));

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetBranchTemplateExcelReportQuery.TemplateId));
    }

    [Fact]
    public void Validate_AcceptsValidSingleTemplateRequest()
    {
        var result = _validator.Validate(CreateQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(25)]
    public void Validate_RejectsUnsupportedRankingCount(int count)
    {
        var result = _validator.Validate(CreateQuery(Guid.NewGuid()) with
        {
            TopWorstQuestionsCount = count
        });

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(GetBranchTemplateExcelReportQuery.TopWorstQuestionsCount));
    }

    private static GetBranchTemplateExcelReportQuery CreateQuery(Guid? templateId)
        => new()
        {
            TemplateId = templateId,
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 21),
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            TopWorstQuestionsCount = 10,
            WorstQuestionsMaxScorePercentage = 40m,
            BestQuestionsMinScorePercentage = 70m,
            Language = "en"
        };
}
