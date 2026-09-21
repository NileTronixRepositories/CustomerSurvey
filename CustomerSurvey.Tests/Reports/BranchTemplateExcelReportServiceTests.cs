using BuildingBlock.Domain.Results;
using ClosedXML.Excel;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplateExcelReportServiceTests
{
    private static readonly string[] ExpectedSheetNames =
    {
        "01 - Summary",
        "02 - Response Matrix",
        "03 - Responses",
        "04 - Answers",
        "05 - Custom Inputs",
        "06 - Question Analysis",
        "07 - Worst Questions",
        "08 - Best Questions"
    };

    [Fact]
    public async Task GenerateAsync_CreatesValidAnalyticalWorkbookWithTypedDetails()
    {
        var model = CreatePopulatedModel();
        var source = new StubBranchTemplatesReportService(model);
        var sut = new BranchTemplateExcelReportService(source);

        var result = await sut.GenerateAsync(CreateRequest(model.SelectedTemplateId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Content);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.Value.ContentType);
        Assert.True(source.LastRequest?.IncludeResponseDetails);

        var tempPath = Path.Combine(Path.GetTempPath(), $"customer-survey-{Guid.NewGuid():N}.xlsx");

        try
        {
            await File.WriteAllBytesAsync(tempPath, result.Value.Content);

            using var workbook = new XLWorkbook(tempPath);

            Assert.Equal(ExpectedSheetNames, workbook.Worksheets.Select(x => x.Name));
            Assert.Equal("Customer Satisfaction", workbook.Worksheet("01 - Summary").Cell("B5").GetString());

            var matrix = workbook.Worksheet("02 - Response Matrix");
            Assert.Equal("01012345678", matrix.Cell(4, 11).GetString());
            Assert.Equal("@", matrix.Cell(4, 11).Style.NumberFormat.Format);
            Assert.Equal(1d, matrix.Cell(4, 10).GetDouble());
            Assert.True(matrix.Cell(5, 10).IsEmpty());

            var answers = workbook.Worksheet("04 - Answers");
            Assert.Equal("Rate the service", answers.Cell(4, 7).GetString());
            Assert.True(answers.Cell(4, 10).IsEmpty());
            Assert.True(answers.Cell(4, 13).IsEmpty());
            Assert.True(answers.Cell(4, 24).GetBoolean());
            Assert.Equal("Root Question", answers.Cell(4, 25).GetString());

            Assert.NotEmpty(matrix.Tables);
            Assert.NotEmpty(workbook.Worksheet("03 - Responses").Tables);
            Assert.NotEmpty(workbook.Worksheet("04 - Answers").Tables);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task GenerateAsync_EmptyPeriodCreatesAllSheetsWithHeadersAndNoFakeRows()
    {
        var templateId = Guid.NewGuid();
        var model = CreateBaseModel(templateId) with
        {
            Templates = new[]
            {
                CreateTemplateSummary(templateId) with
                {
                    TotalResponses = 0,
                    TotalAnswers = 0,
                    TotalScoredAnswers = 0
                }
            }
        };
        var sut = new BranchTemplateExcelReportService(
            new StubBranchTemplatesReportService(model));

        var result = await sut.GenerateAsync(CreateRequest(templateId), CancellationToken.None);

        Assert.True(result.IsSuccess);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);

        Assert.Equal(ExpectedSheetNames, workbook.Worksheets.Select(x => x.Name));
        Assert.Equal("Survey Response Id", workbook.Worksheet("03 - Responses").Cell(3, 2).GetString());
        Assert.True(workbook.Worksheet("03 - Responses").Cell(4, 1).IsEmpty());
        Assert.True(workbook.Worksheet("03 - Responses").AutoFilter.IsEnabled);
    }

    [Fact]
    public async Task GenerateAsync_RejectsMissingTemplateIdBeforeBuildingDataset()
    {
        var source = new StubBranchTemplatesReportService(CreateBaseModel(Guid.NewGuid()));
        var sut = new BranchTemplateExcelReportService(source);

        var result = await sut.GenerateAsync(CreateRequest(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Null(source.LastRequest);
    }

    private static BranchTemplatesPdfReportModel CreatePopulatedModel()
    {
        var templateId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var templateQuestionId = Guid.NewGuid();
        var customInputId = Guid.NewGuid();
        var scoredResponseId = Guid.NewGuid();
        var unscoredResponseId = Guid.NewGuid();

        var question = new BranchTemplatesPdfQuestionAnalytics
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Customer Satisfaction",
            TemplateQuestionId = templateQuestionId,
            QuestionId = questionId,
            QuestionOrder = 1,
            QuestionTextEn = "Rate the service",
            QuestionType = QuestionType.StarRating.ToString(),
            IsRootQuestion = true,
            TotalAnswers = 1,
            SkippedCount = 1,
            AverageValue = 5m,
            ScoreAverageValue = 5m,
            IsScoreIncluded = true
        };

        var rank = new BranchTemplatesPdfQuestionRankItem
        {
            Rank = 1,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = "Customer Satisfaction",
            TemplateQuestionId = templateQuestionId,
            QuestionTextEn = "Rate the service",
            IsRootQuestion = true,
            QuestionType = QuestionType.StarRating.ToString(),
            TotalAnswers = 1,
            AverageScoreValue = 5m,
            SatisfactionPercentage = 100m
        };

        var scoredAnswer = new BranchTemplatesReportAnswer
        {
            ResponseId = scoredResponseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateQuestionId = templateQuestionId,
            QuestionId = questionId,
            QuestionOrder = 1,
            QuestionTextEn = "Rate the service",
            QuestionType = QuestionType.StarRating,
            IsRootQuestion = true,
            StarRatingValue = 5,
            DisplayValue = "5",
            IsScorable = true,
            ScoreValue = 5m,
            IncludedInScore = true,
            ScoreInclusionReason = "Root Question"
        };

        var phoneValue = new BranchTemplatesReportCustomInputValue
        {
            ResponseId = scoredResponseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            CustomInputId = customInputId,
            Name = "Phone",
            Type = TemplateCustomInputType.String,
            Order = 1,
            StringValue = "01012345678"
        };

        return CreateBaseModel(templateId) with
        {
            ExecutiveSummary = new BranchTemplatesPdfExecutiveSummary
            {
                TotalNormalTemplates = 1,
                TotalResponses = 2,
                TotalNormalResponses = 2,
                TotalAnswers = 2,
                TotalScoredAnswers = 1,
                TotalNonScoredAnswers = 1,
                AverageScoreValue = 5m,
                AverageScorePercentage = 100m
            },
            Templates = new[]
            {
                CreateTemplateSummary(templateId) with
                {
                    TotalResponses = 2,
                    TotalAnswers = 2,
                    TotalScoredAnswers = 1,
                    AverageScoreValue = 5m,
                    AverageScorePercentage = 100m
                }
            },
            Questions = new[] { question },
            WorstQuestions = new[] { rank },
            BestQuestions = new[] { rank },
            CustomInputDefinitions = new[]
            {
                new BranchTemplatesReportCustomInputDefinition
                {
                    TemplateId = templateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    CustomInputId = customInputId,
                    Name = "Phone",
                    LabelEn = "Phone",
                    Type = TemplateCustomInputType.String,
                    Order = 1
                }
            },
            Responses = new[]
            {
                new BranchTemplatesReportResponse
                {
                    ResponseId = scoredResponseId,
                    TemplateId = templateId,
                    TemplateNameEn = "Customer Satisfaction",
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = new DateTime(2026, 9, 10, 10, 30, 0, DateTimeKind.Utc),
                    OperatorId = Guid.NewGuid(),
                    OperatorNameEn = "Operator One",
                    IsScored = true,
                    ScoredItemsCount = 1,
                    AverageScoreValue = 5m,
                    ScorePercentage = 100m,
                    CustomInputs = new[] { phoneValue },
                    Answers = new[] { scoredAnswer }
                },
                new BranchTemplatesReportResponse
                {
                    ResponseId = unscoredResponseId,
                    TemplateId = templateId,
                    TemplateNameEn = "Customer Satisfaction",
                    TemplateKind = ReportTemplateKind.Normal,
                    SubmittedOnUtc = new DateTime(2026, 9, 11, 11, 0, 0, DateTimeKind.Utc),
                    OperatorId = Guid.NewGuid(),
                    OperatorNameEn = "Operator Two",
                    IsScored = false
                }
            }
        };
    }

    private static BranchTemplatesPdfReportModel CreateBaseModel(Guid templateId)
        => new()
        {
            Language = "en",
            BranchName = "Cairo Branch",
            GeneratedBy = "Report User",
            GeneratedAtUtc = new DateTime(2026, 9, 21, 15, 30, 0, DateTimeKind.Utc),
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 21),
            SelectedTemplateId = templateId,
            SelectedTemplateName = "Customer Satisfaction",
            SelectedTemplateKind = ReportTemplateKind.Normal,
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            TopWorstQuestionsCount = 10,
            WorstQuestionsMaxScorePercentage = 40m,
            BestQuestionsMinScorePercentage = 70m,
            Templates = new[] { CreateTemplateSummary(templateId) }
        };

    private static BranchTemplatesPdfTemplateSummary CreateTemplateSummary(Guid templateId)
        => new()
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            NameEn = "Customer Satisfaction",
            Status = "Active",
            TotalQuestions = 1,
            RootQuestions = 1
        };

    private static BranchTemplatesPdfReportRequest CreateRequest(Guid? templateId)
        => new()
        {
            BranchId = Guid.NewGuid(),
            GeneratedByApplicationUserId = Guid.NewGuid(),
            GeneratedByName = "Report User",
            TemplateId = templateId,
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 21),
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions,
            TopWorstQuestionsCount = 10,
            WorstQuestionsMaxScorePercentage = 40m,
            BestQuestionsMinScorePercentage = 70m,
            Language = "en"
        };

    private sealed class StubBranchTemplatesReportService : IBranchTemplatesPdfReportService
    {
        private readonly BranchTemplatesPdfReportModel _model;

        public StubBranchTemplatesReportService(BranchTemplatesPdfReportModel model)
        {
            _model = model;
        }

        public BranchTemplatesPdfReportRequest? LastRequest { get; private set; }

        public Task<Result<BranchTemplatesPdfReportModel>> BuildReportModelAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(Result<BranchTemplatesPdfReportModel>.Ok(_model));
        }

        public Task<Result<BranchTemplatesPdfReportFile>> GenerateAsync(
            BranchTemplatesPdfReportRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
