using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.infrastructure.Reports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesPdfRazorReportTests
{
    [Fact]
    public async Task Render_IncludesGraphicsAndBusinessFacingResponseDetails()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var template = new BranchTemplatesPdfTemplateSummary
        {
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            NameEn = "Service Survey",
            TotalQuestions = 1,
            RootQuestions = 1,
            TotalResponses = 1,
            TotalAnswers = 1,
            TotalScoredAnswers = 1,
            AverageScoreValue = 4m,
            AverageScorePercentage = 80m
        };
        var response = new BranchTemplatesReportResponse
        {
            ResponseId = responseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            TemplateNameEn = template.NameEn,
            SubmittedOnUtc = new DateTime(2026, 9, 20, 10, 15, 0, DateTimeKind.Utc),
            OperatorNameEn = "Ahmed Ali",
            IsScored = true,
            AverageScoreValue = 4m,
            ScorePercentage = 80m,
            CustomInputs = new[]
            {
                new BranchTemplatesReportCustomInputValue
                {
                    ResponseId = responseId,
                    TemplateId = templateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    CustomInputId = Guid.NewGuid(),
                    LabelEnSnapshot = "Phone",
                    Type = TemplateCustomInputType.String,
                    StringValue = "01012345678"
                }
            },
            Answers = new[]
            {
                new BranchTemplatesReportAnswer
                {
                    ResponseId = responseId,
                    TemplateId = templateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    QuestionId = Guid.NewGuid(),
                    QuestionTextEn = "Rate the service",
                    QuestionType = QuestionType.StarRating,
                    StarRatingValue = 4,
                    DisplayValue = "4",
                    ScoreValue = 4m,
                    IncludedInScore = true,
                    ScoreInclusionReason = "Root Question"
                }
            }
        };
        var stressResponses = Enumerable.Range(0, 4)
            .Select(index =>
            {
                var currentResponseId = index == 0 ? responseId : Guid.NewGuid();
                var customInputs = Enumerable.Range(1, 6)
                    .Select(inputIndex => new BranchTemplatesReportCustomInputValue
                    {
                        ResponseId = currentResponseId,
                        TemplateId = templateId,
                        TemplateKind = ReportTemplateKind.Normal,
                        CustomInputId = Guid.NewGuid(),
                        LabelEnSnapshot = $"Custom Input {inputIndex}",
                        Type = TemplateCustomInputType.String,
                        Order = inputIndex,
                        StringValue = inputIndex == 1 ? "01012345678" : $"Historical value {inputIndex}"
                    })
                    .ToArray();
                var answers = Enumerable.Range(1, 24)
                    .Select(answerIndex => new BranchTemplatesReportAnswer
                    {
                        ResponseId = currentResponseId,
                        TemplateId = templateId,
                        TemplateKind = ReportTemplateKind.Normal,
                        QuestionId = Guid.NewGuid(),
                        QuestionOrder = answerIndex,
                        QuestionTextEn = $"Question {answerIndex}: Please describe your experience with this service in detail",
                        QuestionTextAr = $"السؤال {answerIndex}: يرجى وصف تجربتك مع هذه الخدمة بالتفصيل",
                        QuestionType = answerIndex == 2 ? QuestionType.FreeText : answerIndex % 6 == 0 ? QuestionType.Image : answerIndex % 5 == 0 ? QuestionType.Voice : answerIndex % 3 == 0 ? QuestionType.Complain : QuestionType.StarRating,
                        StarRatingValue = answerIndex != 2 && answerIndex % 6 != 0 && answerIndex % 5 != 0 && answerIndex % 3 != 0 ? 4 : null,
                        TextAnswer = answerIndex == 2
                            ? "Excellent free text feedback."
                            : answerIndex % 6 != 0 && answerIndex % 3 == 0
                                ? string.Join(' ', Enumerable.Repeat("Long complaint text نص عربي طويل لاختبار التفاف النص وحدود الصفحات.", 8))
                                : null,
                        VoiceFilePath = answerIndex % 5 == 0 ? $"Media/SurveyVoiceAnswers/voice-{answerIndex}.mp3" : null,
                        ImageFilePath = answerIndex % 6 == 0 ? $"https://cdn.example.com/image-{answerIndex}.png" : null,
                        DisplayValue = answerIndex == 2 ? "Excellent free text feedback." : answerIndex % 6 != 0 && answerIndex % 3 == 0 ? "Long complaint" : "4",
                        ScoreValue = answerIndex != 2 && answerIndex % 6 != 0 && answerIndex % 5 != 0 && answerIndex % 3 != 0 ? 4m : null,
                        IncludedInScore = answerIndex != 2 && answerIndex % 6 != 0 && answerIndex % 5 != 0 && answerIndex % 3 != 0,
                        ScoreInclusionReason = answerIndex != 2 && answerIndex % 6 != 0 && answerIndex % 5 != 0 && answerIndex % 3 != 0 ? "Root Question" : "Non-Scorable Question"
                    })
                    .ToArray();

                return response with
                {
                    ResponseId = currentResponseId,
                    SubmittedOnUtc = response.SubmittedOnUtc.AddMinutes(index),
                    OperatorNameEn = index == 3 ? null : $"Operator {index + 1}",
                    CustomInputs = customInputs,
                    Answers = answers
                };
            })
            .ToArray();
        template = template with
        {
            TotalResponses = stressResponses.Length,
            TotalAnswers = stressResponses.Sum(x => x.Answers.Count),
            TotalScoredAnswers = stressResponses.Sum(x => x.Answers.Count(answer => answer.IncludedInScore))
        };
        var model = new BranchTemplatesPdfReportModel
        {
            Language = "en",
            BranchName = "Cairo",
            GeneratedBy = "Report User",
            GeneratedAtUtc = DateTime.UtcNow,
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 22),
            ExecutiveSummary = new BranchTemplatesPdfExecutiveSummary
            {
                TotalNormalTemplates = 1,
                TotalResponses = stressResponses.Length,
                TotalNormalResponses = stressResponses.Length,
                TotalAnswers = stressResponses.Sum(x => x.Answers.Count),
                TotalScoredAnswers = stressResponses.Sum(x => x.Answers.Count(answer => answer.IncludedInScore)),
                AverageScoreValue = 4m,
                AverageScorePercentage = 80m
            },
            Graphics = new BranchTemplatesReportGraphics
            {
                OverallSatisfactionPercentage = 80m,
                AverageScoreValue = 4m,
                TotalResponses = stressResponses.Length,
                ScoredResponses = stressResponses.Length,
                ExcellentResponses = stressResponses.Length,
                RootQuestions = 1,
                IncludedAnswers = stressResponses.Sum(x => x.Answers.Count(answer => answer.IncludedInScore)),
                NonScoredAnswers = stressResponses.Sum(x => x.Answers.Count(answer => !answer.IncludedInScore))
            },
            Templates = new[] { template },
            QuestionGroupAnalytics = new[]
            {
                new BranchTemplatesReportQuestionGroupAnalytics
                {
                    TemplateId = templateId,
                    TemplateKind = ReportTemplateKind.Normal,
                    TemplateNameEn = template.NameEn,
                    QuestionGroupId = Guid.NewGuid(),
                    QuestionGroupNameEn = "Service Quality",
                    QuestionsCount = 1,
                    ScorableQuestionsCount = 1,
                    TotalResponses = stressResponses.Length,
                    ScoredResponsesCount = stressResponses.Length,
                    ScoredItemsCount = stressResponses.Length,
                    AverageScoreValue = 4m,
                    AverageScorePercentage = 80m
                }
            },
            ComplaintAnalytics = new BranchTemplatesReportComplaintAnalytics
            {
                TotalComplaints = 1,
                ResponsesWithComplaints = 1,
                TotalResponses = stressResponses.Length,
                ComplaintRate = 25m,
                Complaints = new[]
                {
                    new BranchTemplatesReportComplaintItem
                    {
                        ResponseId = responseId,
                        TemplateId = templateId,
                        TemplateKind = ReportTemplateKind.Normal,
                        TemplateNameEn = template.NameEn,
                        QuestionId = Guid.NewGuid(),
                        QuestionTextEn = "Tell us what went wrong",
                        ComplaintText = "Long complaint text",
                        SubmittedOnUtc = response.SubmittedOnUtc,
                        OperatorNameEn = "Operator 1"
                    }
                }
            },
            TemplateDetails = new[]
            {
                new BranchTemplatesPdfTemplateDetail { Summary = template }
            },
            Responses = stressResponses
        };
        var contentRoot = FindApiContentRoot();
        var renderer = new RazorViewRenderer(
            new TestWebHostEnvironment(contentRoot),
            NullLogger<RazorViewRenderer>.Instance);

        var html = await renderer.RenderViewToStringAsync(
            "BranchTemplatesPdfReport",
            model,
            isDraft: false);

        Assert.Contains("Graphics", html);
        Assert.Contains("Satisfaction By Question Group", html);
        Assert.Contains("Complaints Analysis", html);
        Assert.Contains("Service Quality", html);
        Assert.Contains("Long complaint text", html);
        Assert.Contains("Response Details", html);
        Assert.Contains("Response #1", html);
        Assert.Contains("Operator 1", html);
        Assert.Contains("01012345678", html);
        Assert.Contains("Question 1: Please describe your experience", html);
        Assert.Contains("Excellent free text feedback.", html);
        Assert.Contains("Included In Score", html);
        Assert.Contains(responseId.ToString(), html, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindApiContentRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "CustomerSurvey.Api");

            if (File.Exists(Path.Combine(candidate, "CustomerSurvey.Api.csproj")))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("CustomerSurvey.Api content root was not found.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
        }

        public string ApplicationName { get; set; } = "CustomerSurvey.Api";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
