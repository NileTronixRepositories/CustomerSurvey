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
    [Fact]
    public async Task BuildReportModel_SelectingAnonymousMemberResolvesWholeSameBranchFamily()
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
            TemplateId = a2.Id,
            TemplateKind = ReportTemplateKind.Anonymous,
            FromDate = new DateOnly(2026, 1, 1),
            ToDate = new DateOnly(2026, 12, 31),
            IncludeResponseDetails = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Templates.Count);
        Assert.Contains(result.Value.Templates, x =>
            x.TemplateId == t1.Id && x.TemplateKind == ReportTemplateKind.Normal);
        Assert.Contains(result.Value.Templates, x =>
            x.TemplateId == a2.Id && x.TemplateKind == ReportTemplateKind.Anonymous);
        Assert.Contains(result.Value.Templates, x =>
            x.TemplateId == t3.Id && x.TemplateKind == ReportTemplateKind.Normal);
        Assert.DoesNotContain(result.Value.Templates, x => x.TemplateId == unrelated.Id);
        Assert.DoesNotContain(result.Value.Templates, x => x.TemplateId == otherBranchMember.Id);
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
