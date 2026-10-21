using Microsoft.Extensions.DependencyInjection;
using BuildingBlock.Application.MultiTenancy;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class SqlHistoryFactAttribute : FactAttribute
{
    public SqlHistoryFactAttribute()
    {
        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("CUSTOMER_SURVEY_RUN_SQL_TESTS") != "1")
            Skip = "Set CUSTOMER_SURVEY_RUN_SQL_TESTS=1 on Windows with SQL Server LocalDB to run isolated persistence checks.";
    }
}

public sealed class SurveyHistorySqlTests
{
    [SqlHistoryFact]
    public async Task Capture_history_backfill_and_score_filters_roundtrip_atomically_in_sql_server()
    {
        var database = "CustomerSurveyHistoryTests_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new PlatformWriteDbContext(options, new Tenant());
        var created = false;
        try
        {
            await db.Database.EnsureCreatedAsync();
            created = true;
            var actor = Guid.NewGuid();
            var branch = Branch.Create("Test branch", null, "HISTORY", null, actor);
            var user = ApplicationUser.Create("operator", "operator@example.test", "Operator", null, null, "unused", UserType.Operator, actor);
            var department = Department.Create("Support", null, actor);
            var op = CustomerSurvey.Domain.Identity.Operator.Create(user.Id, department.Id, actor);
            var group = QuestionGroup.Create(branch.Id, "Original group", null, actor);
            var question = Question.Create(branch.Id, group.Id, "Original question", null, QuestionType.SingleChoice, actor);
            var selected = QuestionOption.Create(question.Id, "Original option", null, 1, 3, actor);
            var unselected = QuestionOption.Create(question.Id, "Unselected option", null, 2, 1, actor);
            var child = Question.Create(branch.Id, group.Id, "Conditional rating", null, QuestionType.StarRating, actor);
            var complaint1 = Question.Create(branch.Id, group.Id, "Original complaint question", null, QuestionType.Complain, actor);
            var complaint2 = Question.Create(branch.Id, group.Id, "Another complaint question", null, QuestionType.Complain, actor);
            var template = Template.Create(branch.Id, "Authorized", null, null, DateTime.UtcNow.AddDays(-10), null, actor);
            var anonymous = AnonymousTemplate.CreateBranchTemplate(branch.Id, "Anonymous", null, null, DateTime.UtcNow.AddDays(-10), null, actor);
            db.AddRange(branch, user, department, op, group, question, selected, unselected, child, complaint1, complaint2, template, anonymous);
            await db.SaveChangesAsync();
            var rootLink = TemplateQuestion.Create(template.Id, question.Id, 1, actor);
            var childLink = TemplateQuestion.Create(template.Id, child.Id, 2, actor);
            var anonLink = AnonymousTemplateQuestion.Create(anonymous.Id, question.Id, 1, actor);
            db.AddRange(rootLink, childLink, anonLink, TemplateQuestion.Create(template.Id, complaint1.Id, 3, actor), TemplateQuestion.Create(template.Id, complaint2.Id, 4, actor));
            db.Add(TemplateQuestionCondition.CreateForSingleChoice(template.Id, rootLink.Id, childLink.Id, selected.Id, 1, actor));
            await db.SaveChangesAsync();

            var response = SurveyResponse.Create(op.Id, template.Id, actor, 3, 5, 60m);
            response.AddAnswer(SurveyAnswer.CreateSingleChoice(response.Id, question.Id, selected.Id));
            response.AddAnswer(SurveyAnswer.CreateStarRating(response.Id, child.Id, 5));
            response.AddAnswer(SurveyAnswer.CreateComplain(response.Id, complaint1.Id, "First complaint"));
            response.AddAnswer(SurveyAnswer.CreateComplain(response.Id, complaint2.Id, "Second complaint"));
            var anonymousResponse = AnonymousSurveyResponse.Create(anonymous.Id, 3, 5, 60m);
            anonymousResponse.AddAnswer(AnonymousSurveyAnswer.CreateSingleChoice(anonymousResponse.Id, anonLink.Id, question.Id, selected.Id));
            db.AddRange(response, anonymousResponse);
            await db.SaveChangesAsync();
            Assert.Equal(2, await db.Set<ResponseSnapshot>().CountAsync());
            var snapshot = response.Snapshot!;
            var payload = snapshot.Payload;
            Assert.Equal(SnapshotProvenance.CapturedAtSubmission, snapshot.Provenance);
            Assert.Equal(4, snapshot.Read().Questions.Count);
            Assert.Equal(2, snapshot.Read().Questions.Single(x => x.QuestionId == question.Id).Options.Count);
            Assert.Single(snapshot.Read().Conditions);
            Assert.Equal(60m, snapshot.RootScorePercentage);
            Assert.Equal(100m, snapshot.LowestScorePercentage);
            Assert.Equal(80m, snapshot.AllScorePercentage);

            template.Update("Renamed template", null, null, template.ActiveFrom, template.ExpireTo);
            db.Update(template);
            await db.SaveChangesAsync();
            Assert.Equal(1, await db.Set<TemplateAvailabilityEvent>().CountAsync(x => x.TemplateId == template.Id));
            complaint1.Update(group.Id, "Renamed complaint question", null, QuestionType.Complain);
            question.Update(group.Id, "Renamed", null, QuestionType.SingleChoice);
            selected.Update("Edited option", null, 1, 1);
            selected.Deactivate();
            var settings = BranchEvaluationSettings.Create(branch.Id, actor);
            settings.Update(65m, 90m, 60m, 90m, actor);
            db.Add(settings);
            template.Deactivate();
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var stored = await db.Set<SurveyResponse>().Include(x => x.Snapshot).SingleAsync();
            Assert.Equal(payload, stored.Snapshot!.Payload);
            Assert.Equal(60m, stored.ScorePercentage);
            Assert.Equal(3, stored.ActualScore);
            Assert.Equal(5, stored.MaxScore);
            Assert.Equal(2, await db.Set<TemplateAvailabilityEvent>().CountAsync(x => x.TemplateId == template.Id));
            var events = await db.Set<TemplateAvailabilityEvent>().Where(x => x.TemplateId == template.Id).ToArrayAsync();
            Assert.True(TemplateAvailabilityIntervals.WasAvailable(events, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)));

            var spec = new GetBranchSurveyResponsesPaginationSpec(branch.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1),
                new GetBranchSurveyResponsesPaginationQuery { ScoreCalculationMode = ScoreCalculationMode.AllAnsweredQuestions,
                    MinScorePercentage = 80m, MaxScorePercentage = 80m, SatisfactionCategory = SatisfactionCategory.Neutral,
                    QuestionId = child.Id, ScoreIncludedOnly = true });
            var query = db.Set<SurveyResponse>().Where(spec.Criteria).Select(spec.Selector);
            Assert.Contains("AllScorePercentage", query.ToQueryString());
            var item = Assert.Single(await query.ToListAsync());
            Assert.Equal(80m, item.ScorePercentage);
            Assert.True(item.RequestedModeAvailable);

            var resolver = new PlatformWriteReadDbContextResolver(db);
            var complaintReader = new CustomerSurvey.infrastructure.Reports.SurveyDashboardComplaintReadService(
                new CustomerSurvey.infrastructure.Repositories.PlatformWriteReadRepository<SurveyAnswer>(resolver),
                new CustomerSurvey.infrastructure.Repositories.PlatformWriteReadRepository<AnonymousSurveyAnswer>(resolver),
                new CustomerSurvey.infrastructure.Repositories.PlatformWriteReadRepository<SurveyResponse>(resolver),
                new CustomerSurvey.infrastructure.Repositories.PlatformWriteReadRepository<AnonymousSurveyResponse>(resolver));
            var complaints = await complaintReader.ReadAsync(new CustomerSurvey.Application.Abstraction.Reports.SurveyDashboardComplaintReadRequest(
                true, true, branch.Id, template.Id, anonymous.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), 1, 10), CancellationToken.None);
            Assert.Equal(2, complaints.TotalComplaints);
            Assert.Equal(1, complaints.ResponsesWithComplaints);
            Assert.Equal("Original complaint question", complaints.PageItems.Single(x => x.QuestionId == complaint1.Id).QuestionTextEn);
            var departmentSpec = new CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination.GetDepartmentSurveyResponsesPaginationSpec(
                department.Id, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1),
                new CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination.GetDepartmentSurveyResponsesPaginationQuery());
            var complaintSpec = new ComplaintItemsSpecification<CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination.DepartmentSurveyResponsePaginationItemResponse>(departmentSpec, 1, 10, null);
            var complaintRows = await db.Set<SurveyAnswer>().Where(complaintSpec.Criteria).Select(complaintSpec.Selector).ToListAsync();
            Assert.Equal(2, complaintRows.Count);
            Assert.Single(complaintRows.Select(x => x.SurveyResponseId).Distinct());

            using var services = new ServiceCollection()
                .AddSingleton<BuildingBlock.Application.Abstraction.Persistence.IDbContextResolver<CustomerSurvey.Application.Abstraction.Presistence.PlatformWriteReadMarker>>(resolver)
                .AddTransient(typeof(CustomerSurvey.Application.Abstraction.Presistence.IWriteReadRepository<>), typeof(CustomerSurvey.infrastructure.Repositories.PlatformWriteReadRepository<>))
                .AddSingleton<ISurveyReportScoringService>(new SurveyReportScoringService())
                .AddSingleton<CustomerSurvey.Application.Abstraction.Reports.IPdfService>(new UnusedPdfService())
                .AddTransient<CustomerSurvey.Application.Abstraction.Reports.ITemplateAvailabilityReader, CustomerSurvey.infrastructure.Reports.TemplateAvailabilityReader>()
                .BuildServiceProvider();
            var reportService = ActivatorUtilities.CreateInstance<CustomerSurvey.infrastructure.Reports.BranchTemplatesPdfReportService>(services);
            var reportRequest = new CustomerSurvey.Application.Abstraction.Reports.BranchTemplatesPdfReportRequest
            {
                BranchId = branch.Id, TemplateId = template.Id, TemplateKind = ReportTemplateKind.Normal,
                GeneratedByApplicationUserId = actor, GeneratedByName = "Admin", ScoreCalculationMode = ScoreCalculationMode.AllAnsweredQuestions,
                FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), ToDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            var report = await reportService.BuildReportModelAsync(reportRequest, CancellationToken.None);
            Assert.True(report.IsSuccess);
            Assert.Equal(80m, report.Value.ExecutiveSummary.AverageScorePercentage);
            Assert.Equal("MediumRisk", report.Value.ExecutiveSummary.RiskLevel);
            Assert.Equal(1, report.Value.ExecutiveSummary.NeutralResponses);
            Assert.Equal(2, report.Value.ComplaintAnalytics.TotalComplaints);
            Assert.Equal(1, report.Value.Availability.AvailableTemplatesDuringPeriod);
            Assert.Equal(0, report.Value.Availability.CurrentlyAvailableTemplatesCount);
            Assert.Equal(80m, Assert.Single(report.Value.QuestionGroupAnalytics).AverageScorePercentage);
            Assert.Equal("Original question", report.Value.Questions.Single(x => x.QuestionId == question.Id).QuestionTextEn);
            var excel = await new CustomerSurvey.infrastructure.Reports.BranchTemplateExcelReportService(reportService).GenerateAsync(reportRequest, CancellationToken.None);
            Assert.True(excel.IsSuccess);
            Assert.NotEmpty(excel.Value.Content);

            var maintenance = new SurveyHistoryMaintenance(db);
            var first = await maintenance.RunBatchAsync(10);
            Assert.Equal(0, first.AuthorizedSnapshots + first.AnonymousSnapshots + first.AvailabilityBaselines);
            var second = await maintenance.RunBatchAsync(10);
            Assert.Equal(first, second);

            // Simulate an existing response without a snapshot; the maintenance operation must preserve its stored score.
            await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO [SurveyResponse] ([Id], [OperatorId], [TemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage], [CreatedByApplicationUserId]) VALUES ({Guid.NewGuid()}, {op.Id}, {template.Id}, {DateTime.UtcNow}, {DateTime.UtcNow}, {3}, {3}, {100m}, {actor})");
            var legacy = await maintenance.RunBatchAsync(10);
            Assert.Equal(1, legacy.AuthorizedSnapshots);
            var reconstructed = await db.Set<ResponseSnapshot>().SingleAsync(x => x.Provenance == SnapshotProvenance.LegacyIncomplete);
            Assert.Equal(100m, reconstructed.Read().ScorePercentage);
            Assert.Null(reconstructed.AllScorePercentage);
            Assert.False((await maintenance.RunBatchAsync(10)).MayHaveMore);
        }
        finally
        {
            if (created && database.StartsWith("CustomerSurveyHistoryTests_", StringComparison.Ordinal))
                await db.Database.EnsureDeletedAsync();
        }
    }

    private sealed class UnusedPdfService : CustomerSurvey.Application.Abstraction.Reports.IPdfService
    {
        public Task<byte[]> GeneratePdfAsync<T>(string viewName, T model, CustomerSurvey.Application.Abstraction.Reports.PdfRenderOptions options, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<byte[]> GeneratePdfAsync(string viewName, CustomerSurvey.Application.Abstraction.Reports.PdfRenderOptions options, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public byte[] MergePdfByteArrays(byte[] first, byte[] second) => throw new NotSupportedException();
        public byte[] MergePdfByteArrays(IReadOnlyCollection<byte[]> pdfs) => throw new NotSupportedException();
        public bool IsPdfEmpty(byte[] bytes) => throw new NotSupportedException();
    }

    private sealed class Tenant : ICurrentTenantContext
    {
        public TenantMode Mode => default;
        public Guid? AccountId => null;
        public Guid? UserId => null;
        public bool IsAuthenticated => true;
        public string? Role => null;
        public BuildingBlock.Application.Abstraction.Security.UserType UserType => BuildingBlock.Application.Abstraction.Security.UserType.Unknown;
        public bool HasAccount => false;
        public bool IsPlatformAdmin => true;
    }
}
