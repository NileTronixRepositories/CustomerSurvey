using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class TemplateFamilyReportResolutionTests
{
    [Theory]
    [InlineData(ReportTemplateKind.Normal)]
    [InlineData(ReportTemplateKind.Anonymous)]
    public async Task BuildReportModel_SelectingFamilyMemberReturnsOnlyExactTemplateAndExcelWorks(ReportTemplateKind kind)
    {
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, userId);
        var otherBranch = Branch.Create("Alex", null, "ALX", null, userId);
        var t1 = CreateAuthorized(branch.Id, "T1", userId, familyId);
        var a2 = CreateAnonymous(branch.Id, "A2", userId, familyId);
        var t3 = CreateAuthorized(branch.Id, "T3", userId, familyId);
        var unrelated = CreateAuthorized(branch.Id, "T4", userId, null);
        var otherBranchMember = CreateAuthorized(otherBranch.Id, "Other", userId, familyId);

        var branches = new InMemoryReadRepository<Branch>();
        branches.Add(branch);
        branches.Add(otherBranch);
        var templates = new InMemoryReadRepository<Template>();
        templates.AddRange(new[] { t1, t3, unrelated, otherBranchMember });
        var anonymousTemplates = new InMemoryReadRepository<AnonymousTemplate>();
        anonymousTemplates.Add(a2);

        var service = new BranchTemplatesPdfReportService(
            branches,
            templates,
            anonymousTemplates,
            new InMemoryReadRepository<SurveyResponse>(),
            new InMemoryReadRepository<SurveyAnswer>(),
            new InMemoryReadRepository<SurveyResponseCustomInputValue>(),
            new InMemoryReadRepository<AnonymousSurveyResponse>(),
            new InMemoryReadRepository<AnonymousSurveyAnswer>(),
            new InMemoryReadRepository<AnonymousSurveyResponseCustomInputValue>(),
            new InMemoryReadRepository<TemplateQuestion>(),
            new InMemoryReadRepository<AnonymousTemplateQuestion>(),
            new InMemoryReadRepository<TemplateCustomInput>(),
            new InMemoryReadRepository<AnonymousTemplateCustomInput>(),
            new InMemoryReadRepository<TemplateQuestionCondition>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(),
            new InMemoryReadRepository<QuestionOption>(),
            new SurveyReportScoringService(),
            new UnusedPdfService());

        var result = await service.BuildReportModelAsync(new BranchTemplatesPdfReportRequest
        {
            BranchId = branch.Id,
            GeneratedByApplicationUserId = userId,
            GeneratedByName = "Admin",
            TemplateId = kind == ReportTemplateKind.Normal ? t1.Id : a2.Id,
            TemplateKind = kind,
            FromDate = new DateOnly(2026, 1, 1),
            ToDate = new DateOnly(2026, 12, 31),
            IncludeResponseDetails = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var selected = Assert.Single(result.Value.Templates);
        Assert.Equal(kind == ReportTemplateKind.Normal ? t1.Id : a2.Id, selected.TemplateId);
        Assert.Equal(kind, selected.TemplateKind);
        var excel = await new BranchTemplateExcelReportService(service).GenerateAsync(new BranchTemplatesPdfReportRequest
        {
            BranchId = branch.Id, GeneratedByApplicationUserId = userId, GeneratedByName = "Admin",
            TemplateId = selected.TemplateId, TemplateKind = kind,
            FromDate = new DateOnly(2026, 1, 1), ToDate = new DateOnly(2026, 12, 31)
        }, CancellationToken.None);
        Assert.True(excel.IsSuccess);
        Assert.NotEmpty(excel.Value.Content);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Complaint_totals_count_answers_without_requiring_response_details(bool includeDetails)
    {
        var actor = Guid.NewGuid();
        var branch = Branch.Create("Cairo", null, "CAI", null, actor);
        var template = AnonymousTemplate.CreateBranchTemplate(branch.Id, "Survey", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        var response = AnonymousSurveyResponse.Create(template.Id, 0, 0, 0m);
        var branches = new InMemoryReadRepository<Branch>(); branches.Add(branch);
        var templates = new InMemoryReadRepository<AnonymousTemplate>(); templates.Add(template);
        var responses = new InMemoryReadRepository<AnonymousSurveyResponse>(); responses.Add(response);
        var answers = new InMemoryReadRepository<AnonymousSurveyAnswer>();
        foreach (var (type, text) in new[] { (QuestionType.Complain, "First complaint"), (QuestionType.Complain, "Second complaint"),
                     (QuestionType.Complain, "  "), (QuestionType.FreeText, "ordinary feedback") })
        {
            var question = Question.Create(branch.Id, Guid.NewGuid(), "Question", null, type, actor);
            var answer = type == QuestionType.Complain
                ? AnonymousSurveyAnswer.CreateComplain(response.Id, Guid.NewGuid(), question.Id, text)
                : AnonymousSurveyAnswer.CreateFreeText(response.Id, Guid.NewGuid(), question.Id, text);
            typeof(AnonymousSurveyAnswer).GetProperty(nameof(AnonymousSurveyAnswer.Question))!.SetValue(answer, question);
            answers.Add(answer);
        }
        var service = new BranchTemplatesPdfReportService(branches, new InMemoryReadRepository<Template>(), templates,
            new InMemoryReadRepository<SurveyResponse>(), new InMemoryReadRepository<SurveyAnswer>(),
            new InMemoryReadRepository<SurveyResponseCustomInputValue>(), responses, answers,
            new InMemoryReadRepository<AnonymousSurveyResponseCustomInputValue>(), new InMemoryReadRepository<TemplateQuestion>(),
            new InMemoryReadRepository<AnonymousTemplateQuestion>(), new InMemoryReadRepository<TemplateCustomInput>(),
            new InMemoryReadRepository<AnonymousTemplateCustomInput>(), new InMemoryReadRepository<TemplateQuestionCondition>(),
            new InMemoryReadRepository<AnonymousTemplateQuestionCondition>(), new InMemoryReadRepository<QuestionOption>(),
            new SurveyReportScoringService(), new UnusedPdfService());
        var result = await service.BuildReportModelAsync(new BranchTemplatesPdfReportRequest
        {
            BranchId = branch.Id, TemplateId = template.Id, TemplateKind = ReportTemplateKind.Anonymous,
            GeneratedByApplicationUserId = actor, GeneratedByName = "Admin", IncludeResponseDetails = includeDetails,
            FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), ToDate = DateOnly.FromDateTime(DateTime.UtcNow)
        }, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ComplaintAnalytics.TotalComplaints);
        Assert.Equal(1, result.Value.ComplaintAnalytics.ResponsesWithComplaints);
        Assert.Equal(100m, result.Value.ComplaintAnalytics.ComplaintRate);
        Assert.Equal(2, result.Value.ExecutiveSummary.TotalComplaints);
        Assert.False(result.Value.ExecutiveSummary.HasScoreData);
        Assert.Equal("NoData", result.Value.ExecutiveSummary.RiskLevel);
        if (!includeDetails) Assert.Empty(result.Value.Responses);
    }

    private static Template CreateAuthorized(
        Guid branchId,
        string name,
        Guid userId,
        Guid? familyId)
        => Template.Create(
            branchId, name, null, null, DateTime.UtcNow, null, userId,
            templateFamilyId: familyId);

    private static AnonymousTemplate CreateAnonymous(
        Guid branchId,
        string name,
        Guid userId,
        Guid familyId)
        => AnonymousTemplate.CreateBranchTemplate(
            branchId, name, null, null, DateTime.UtcNow, null, userId,
            templateFamilyId: familyId);

    private sealed class UnusedPdfService : IPdfService
    {
        public Task<byte[]> GeneratePdfAsync<TModel>(
            string viewName,
            TModel model,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<byte[]> GeneratePdfAsync(
            string viewName,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public byte[] MergePdfByteArrays(byte[] pdf1, byte[] pdf2)
            => throw new NotSupportedException();

        public byte[] MergePdfByteArrays(IReadOnlyCollection<byte[]> pdfs)
            => throw new NotSupportedException();

        public bool IsPdfEmpty(byte[] pdfBytes)
            => throw new NotSupportedException();
    }
}
