using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Repositories;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class VisitTimeReportingSqlTests
{
    [SqlHistoryFact]
    public async Task MigrationAndTrendsUsePersistedVisitDatesAndPreserveLegacyDataAndScope()
    {
        var database = "CustomerSurveyVisitTimeTests_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true")
            .Options;
        await using var db = new PlatformWriteDbContext(options, new Tenant());
        var previousMigration = "20261021113627_AddImmutableSnapshotScoreQueries";
        try
        {
            await db.GetService<IMigrator>().MigrateAsync(previousMigration);

            var actor = Guid.NewGuid();
            var branch = Branch.Create("Home branch", null, "HOME", null, actor);
            var otherBranch = Branch.Create("Other branch", null, "OTHER", null, actor);
            var user = ApplicationUser.Create("operator", "operator@example.test", "Operator", null, null, "unused", UserType.Operator, actor);
            var department = Department.Create("Support", null, actor);
            var otherDepartment = Department.Create("Other", null, actor);
            var op = Operator.Create(user.Id, department.Id, actor);
            var otherUser = ApplicationUser.Create("other", "other@example.test", "Other", null, null, "unused", UserType.Operator, actor);
            var otherOp = Operator.Create(otherUser.Id, otherDepartment.Id, actor);
            db.AddRange(branch, otherBranch, user, otherUser, department, otherDepartment, op, otherOp);
            await db.SaveChangesAsync();

            var from = new DateTime(2026, 11, 1);
            var to = new DateTime(2026, 11, 10);
            var normalId = Guid.NewGuid();
            var anonymousId = Guid.NewGuid();
            var globalId = Guid.NewGuid();
            var otherBranchTemplateId = Guid.NewGuid();
            var ordinaryId = Guid.NewGuid();
            var legacyResponseId = Guid.NewGuid();
            var legacyValueId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [Template] ([Id], [BranchId], [NameEn], [IsActive], [ActiveFrom], [CreatedOnUtc], [CreatedByApplicationUserId])
                VALUES ({normalId}, {branch.Id}, N'Legacy normal', 0, {from}, {from}, {actor}),
                       ({otherBranchTemplateId}, {otherBranch.Id}, N'Other branch', 1, {from}, {from}, {actor});
                INSERT INTO [AnonymousTemplate] ([Id], [BranchId], [NameEn], [Scope], [IsActive], [ActiveFrom], [CreatedOnUtc], [CreatedByApplicationUserId])
                VALUES ({anonymousId}, {branch.Id}, N'Legacy branch anonymous', 1, 1, {from}, {from}, {actor}),
                       ({globalId}, NULL, N'Legacy global anonymous', 2, 0, {from}, {from}, {actor});
                INSERT INTO [TemplateCustomInput] ([Id], [TemplateId], [LabelEn], [Type], [Order], [IsRequired], [IsActive], [CreatedOnUtc], [CreatedByApplicationUserId])
                VALUES ({ordinaryId}, {normalId}, N'Visit Time', 1, 2147483647, 0, 1, {from}, {actor});
                INSERT INTO [SurveyResponse] ([Id], [OperatorId], [TemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage], [CreatedByApplicationUserId])
                VALUES ({legacyResponseId}, {op.Id}, {normalId}, {from.AddDays(6)}, {from}, 5, 5, 100, {actor});
                INSERT INTO [SurveyResponseCustomInputValue] ([Id], [SurveyResponseId], [TemplateCustomInputId], [NameSnapshot], [LabelEnSnapshot], [TypeSnapshot], [StringValue], [CreatedOnUtc])
                VALUES ({legacyValueId}, {legacyResponseId}, {ordinaryId}, N'Visit Time', N'Visit Time', 1, N'2026-11-07', {from});
                """);

            await db.Database.MigrateAsync();
            var templateInputs = await db.Set<TemplateCustomInput>().Where(x => x.TemplateId == normalId).ToArrayAsync();
            var normalVisit = Assert.Single(templateInputs, x => x.SystemFieldKey == TemplateCustomInputSystemField.VisitTime);
            Assert.Equal((1, false, true, actor), (normalVisit.Order, normalVisit.IsRequired, normalVisit.IsActive, normalVisit.CreatedByApplicationUserId));
            Assert.Equal(int.MaxValue, Assert.Single(templateInputs, x => x.Id == ordinaryId).Order);
            var legacyValue = await db.Set<SurveyResponseCustomInputValue>().SingleAsync(x => x.Id == legacyValueId);
            Assert.Null(legacyValue.DateValue);
            Assert.Equal("2026-11-07", legacyValue.StringValue);
            Assert.False(await db.Set<SurveyResponse>().Where(x => x.Id == legacyResponseId).AnyAsync(x => x.Snapshot != null));

            var anonymousFields = await db.Set<AnonymousTemplateCustomInput>().ToArrayAsync();
            var anonymousVisit = Assert.Single(anonymousFields, x => x.AnonymousTemplateId == anonymousId);
            var globalVisit = Assert.Single(anonymousFields, x => x.AnonymousTemplateId == globalId);
            Assert.All(anonymousFields, x => Assert.Equal(TemplateCustomInputSystemField.VisitTime, x.SystemFieldKey));
            Assert.Equal(2, anonymousFields.Length);
            var otherVisit = await db.Set<TemplateCustomInput>().SingleAsync(x => x.TemplateId == otherBranchTemplateId);
            var publicField = Assert.Single(await Repo<AnonymousTemplateCustomInput>().ListAsync(
                new GetPublicAnonymousTemplateCustomInputsSpec(anonymousId)));
            Assert.True(publicField.IsSystemField);
            Assert.Equal("VisitTime", publicField.SystemFieldKey);

            var inside = new DateOnly(2026, 11, 5);
            var normalInside = await InsertNormal(normalId, normalVisit.Id, op.Id, to.AddDays(5), inside, 75.01m);
            var anonymousInside = await InsertAnonymous(anonymousId, anonymousVisit.Id, from.AddDays(7), inside, 50m);
            var anonymousUnscored = await InsertAnonymous(anonymousId, anonymousVisit.Id, to.AddDays(1), inside, null);
            var normalOutside = await InsertNormal(normalId, normalVisit.Id, op.Id, from.AddDays(4), inside.AddDays(-5), 20m);
            await InsertAnonymous(anonymousId, anonymousVisit.Id, to.AddDays(1), new DateOnly(2025, 11, 9), 80m);
            var outsideBranch = await InsertNormal(otherBranchTemplateId, otherVisit.Id, op.Id, to.AddDays(1), inside, 100m);
            var outsideDepartment = await InsertNormal(normalId, normalVisit.Id, otherOp.Id, to.AddDays(1), inside, 10m);
            await InsertAnonymous(globalId, globalVisit.Id, to.AddDays(1), inside, 20m);

            var otherDateInputId = Guid.NewGuid();
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [TemplateCustomInput] ([Id], [TemplateId], [LabelEn], [Type], [Order], [IsRequired], [IsActive], [CreatedOnUtc], [CreatedByApplicationUserId])
                VALUES ({otherDateInputId}, {normalId}, N'Other date', 3, 2, 0, 1, {from}, {actor});
                INSERT INTO [SurveyResponseCustomInputValue] ([Id], [SurveyResponseId], [TemplateCustomInputId], [NameSnapshot], [TypeSnapshot], [DateValue], [CreatedOnUtc])
                VALUES (NEWID(), {normalOutside}, {otherDateInputId}, N'Other date', 3, {inside}, {from});
                """);

            var scope = new BranchScope(branch.Id, actor);
            var currentUser = new TestCurrentUser(actor);
            var scoring = new SurveyReportScoringService();
            var dashboard = new GetSurveyDashboardQueryHandler(
                Repo<SuperAdmin>(), Repo<Branch>(), Repo<Template>(), Repo<AnonymousTemplate>(),
                Repo<SurveyResponse>(), Repo<SurveyAnswer>(), Repo<SurveyResponseCustomInputValue>(),
                Repo<AnonymousSurveyResponse>(), Repo<AnonymousSurveyAnswer>(), Repo<AnonymousSurveyResponseCustomInputValue>(),
                Repo<TemplateQuestion>(), Repo<AnonymousTemplateQuestion>(), Repo<TemplateQuestionCondition>(),
                Repo<AnonymousTemplateQuestionCondition>(), Repo<QuestionOption>(), scoring, scope, currentUser);
            var listing = new GetSurveyResponsesPaginationQueryHandler(
                Repo<SuperAdmin>(), Repo<Branch>(), Repo<Template>(), Repo<AnonymousTemplate>(),
                Repo<SurveyResponse>(), Repo<SurveyAnswer>(), Repo<SurveyResponseCustomInputValue>(),
                Repo<AnonymousSurveyResponse>(), Repo<AnonymousSurveyAnswer>(), Repo<AnonymousSurveyResponseCustomInputValue>(),
                Repo<TemplateQuestion>(), Repo<AnonymousTemplateQuestion>(), Repo<TemplateQuestionCondition>(),
                Repo<AnonymousTemplateQuestionCondition>(), Repo<QuestionOption>(), scoring, scope, currentUser);

            var day = (await dashboard.Handle(new GetSurveyDashboardQuery { From = from, To = to }, default)).Value;
            Assert.Equal(3, day.Summary.TotalResponses);
            Assert.Equal(56.67m, day.Summary.AverageScorePercentage);
            var point = Assert.Single(day.SatisfactionTrend);
            Assert.Equal(("2026-11-05", 4, 3, 45.00m), (point.Period, point.ResponsesCount, point.ScoredResponsesCount, point.AverageScorePercentage));
            Assert.DoesNotContain(day.CustomInputSegments, x => x.CustomInputId == normalVisit.Id || x.CustomInputId == anonymousVisit.Id);
            var linkFilters = QueryHelpers.ParseQuery(new Uri("https://example.test" + point.DetailsNavigation!.Path).Query);
            Assert.Equal("VisitTime", linkFilters["dateBasis"].ToString());
            var drilldown = (await listing.Handle(new GetSurveyResponsesPaginationQuery
            {
                From = DateTime.Parse(linkFilters["from"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                To = DateTime.Parse(linkFilters["to"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                DateBasis = Enum.Parse<ResponseDateBasis>(linkFilters["dateBasis"].ToString())
            }, default)).Value;
            Assert.Equal(point.ResponsesCount, drilldown.TotalItems);
            Assert.Equal(new[] { normalInside, anonymousInside, anonymousUnscored, outsideDepartment }.Order(),
                drilldown.Data.Select(x => x.ResponseId).Order());
            Assert.All(drilldown.Data, x => Assert.Equal(inside, Assert.Single(x.CustomInputsPreview).DateValue));

            var legacyList = (await listing.Handle(new GetSurveyResponsesPaginationQuery { From = from, To = to }, default)).Value;
            Assert.Equal(new[] { normalOutside, legacyResponseId, anonymousInside }.Order(), legacyList.Data.Select(x => x.ResponseId).Order());

            var monthly = (await dashboard.Handle(new GetSurveyDashboardQuery
            {
                From = new DateTime(2025, 11, 9), To = to, GroupBy = DashboardGroupBy.Month, Source = SurveyDashboardSource.Anonymous
            }, default)).Value.SatisfactionTrend.ToArray();
            Assert.Equal(new[] { "2025-11", "2026-11" }, monthly.Select(x => x.Period));
            Assert.Equal(new[] { 1, 2 }, monthly.Select(x => x.ResponsesCount));
            Assert.Equal(80m, monthly[0].AverageScorePercentage);
            Assert.Contains("from=2025-11-09", monthly[0].DetailsNavigation!.Path);
            Assert.Contains("to=2026-11-10", monthly[1].DetailsNavigation!.Path);
            var empty = (await dashboard.Handle(new GetSurveyDashboardQuery
            {
                From = from.AddDays(5), To = to, Source = SurveyDashboardSource.Internal
            }, default)).Value;
            Assert.Empty(empty.SatisfactionTrend);

            var branchDashboard = new GetBranchDashboardQueryHandler(
                Repo<Branch>(), Repo<Template>(), Repo<SurveyResponse>(), Repo<SurveyAnswer>(),
                Repo<SurveyResponseCustomInputValue>(), scope, currentUser);
            var branchResult = (await branchDashboard.Handle(new GetBranchDashboardQuery
            {
                From = DateOnly.FromDateTime(from), To = DateOnly.FromDateTime(to)
            }, default)).Value;
            var branchPoint = Assert.Single(branchResult.SatisfactionTrend);
            Assert.Equal(("2026-11-05", 2, 42.51m), (branchPoint.Period, branchPoint.ResponsesCount, branchPoint.AverageScorePercentage));
            Assert.Contains("dateBasis=VisitTime", branchPoint.DetailsNavigation!.Path);
            Assert.Contains(branchResult.CustomInputSegments, x => x.CustomInputId == otherDateInputId);

            var branchReport = new GetBranchSatisfactionReportQueryHandler(
                Repo<Template>(), Repo<SurveyResponse>(), Repo<SurveyAnswer>(), scope, currentUser);
            var branchReportPoint = Assert.Single((await branchReport.Handle(new GetBranchSatisfactionReportQuery
            {
                From = DateOnly.FromDateTime(from), To = DateOnly.FromDateTime(to)
            }, default)).Value.Trend);
            Assert.Equal((inside, 2, 42.51m), (branchReportPoint.Date, branchReportPoint.ResponsesCount, branchReportPoint.Score));
            Assert.Contains("dateBasis=VisitTime", branchReportPoint.DetailsNavigation!.Path);

            var anonymousDashboard = new GetAnonymousTemplateDashboardQueryHandler(
                Repo<Branch>(), Repo<AnonymousTemplate>(), Repo<AnonymousSurveyResponse>(), Repo<AnonymousSurveyAnswer>(),
                Repo<AnonymousSurveyResponseCustomInputValue>(), scope, currentUser);
            var anonymousResult = (await anonymousDashboard.Handle(new GetAnonymousTemplateDashboardQuery
            {
                AnonymousTemplateId = anonymousId, From = DateOnly.FromDateTime(from), To = DateOnly.FromDateTime(to),
                CriticalScoreThreshold = 60m
            }, default)).Value;
            var anonymousPoint = Assert.Single(anonymousResult.SatisfactionTrend);
            Assert.Equal(("2026-11-05", 1, 50m), (anonymousPoint.Period, anonymousPoint.ResponsesCount, anonymousPoint.AverageScorePercentage));
            Assert.Contains("dateBasis=VisitTime", anonymousPoint.DetailsNavigation!.Path);
            Assert.Contains("fromDate=2026-11-05", anonymousPoint.DetailsNavigation.Path);
            Assert.Contains("toDate=2026-11-06", anonymousPoint.DetailsNavigation.Path);
            Assert.Equal(inside, Assert.Single(Assert.Single(anonymousResult.CriticalResponses).CustomInputs).DateValue);

            var departmentAdmins = new InMemoryReadRepository<DepartmentAdmin>();
            var departmentAdmin = DepartmentAdmin.Create(actor, department.Id, actor);
            typeof(DepartmentAdmin).GetProperty(nameof(DepartmentAdmin.Department))!.SetValue(departmentAdmin, department);
            departmentAdmins.Add(departmentAdmin);
            var departmentDashboard = new GetDepartmentDashboardQueryHandler(
                departmentAdmins, Repo<Operator>(), Repo<OperatorTemplate>(), Repo<Template>(),
                Repo<SurveyResponse>(), Repo<SurveyAnswer>(), Repo<SurveyResponseCustomInputValue>(), currentUser);
            var departmentPoint = Assert.Single((await departmentDashboard.Handle(new GetDepartmentDashboardQuery
            {
                From = DateOnly.FromDateTime(from), To = DateOnly.FromDateTime(to)
            }, default)).Value.SatisfactionTrend);
            Assert.Equal(("2026-11-05", 2, 87.51m), (departmentPoint.Period, departmentPoint.ResponsesCount, departmentPoint.AverageScorePercentage));
            Assert.Contains("dateBasis=VisitTime", departmentPoint.DetailsNavigation!.Path);

            var superAdmins = new InMemoryReadRepository<SuperAdmin>();
            superAdmins.Add(SuperAdmin.Create(actor, actor));
            var systemDashboard = new GetSystemDashboardQueryHandler(
                superAdmins, Repo<Branch>(), Repo<Department>(), Repo<Operator>(), Repo<Template>(),
                Repo<SurveyResponse>(), Repo<SurveyAnswer>(), Repo<SurveyResponseCustomInputValue>(), currentUser);
            var systemPoint = Assert.Single((await systemDashboard.Handle(new GetSystemDashboardQuery
            {
                From = DateOnly.FromDateTime(from), To = DateOnly.FromDateTime(to), BranchId = branch.Id, DepartmentId = department.Id
            }, default)).Value.SatisfactionTrend);
            Assert.Equal(("2026-11-05", 1, 75.01m), (systemPoint.Period, systemPoint.ResponsesCount, systemPoint.AverageScorePercentage));
            Assert.Contains("dateBasis=VisitTime", systemPoint.DetailsNavigation!.Path);

            var basis = ResponseDateBasis.VisitTime;
            var exclusive = to.AddDays(1);
            var branchList = await Repo<SurveyResponse>().ListAsync(new GetBranchSurveyResponsesPaginationSpec(
                branch.Id, from, exclusive, new GetBranchSurveyResponsesPaginationQuery { DateBasis = basis }));
            Assert.Equal(new[] { normalInside, outsideDepartment }.Order(), branchList.Select(x => x.SurveyResponseId).Order());
            var departmentList = await Repo<SurveyResponse>().ListAsync(new GetDepartmentSurveyResponsesPaginationSpec(
                department.Id, from, exclusive, new GetDepartmentSurveyResponsesPaginationQuery { DateBasis = basis }));
            Assert.Equal(new[] { normalInside, outsideBranch }.Order(), departmentList.Select(x => x.SurveyResponseId).Order());
            var systemList = await Repo<SurveyResponse>().ListAsync(new GetSystemSurveyResponsesPaginationSpec(
                from, exclusive, new GetSystemSurveyResponsesPaginationQuery { DateBasis = basis, BranchId = branch.Id, DepartmentId = department.Id }));
            Assert.Equal(normalInside, Assert.Single(systemList).SurveyResponseId);

            var templateList = await Repo<AnonymousSurveyResponse>().ListAsync(new GetAnonymousTemplateResponsesPaginationSpec(
                new GetAnonymousTemplateResponsesPaginationQuery
                { AnonymousTemplateId = anonymousId, FromDate = from, ToDate = exclusive, DateBasis = basis, IsScored = true }));
            Assert.Equal(anonymousInside, Assert.Single(templateList).AnonymousSurveyResponseId);
            var anonymousList = await Repo<AnonymousSurveyResponse>().ListAsync(new GetBranchAnonymousResponsesPaginationSpec(
                branch.Id, from, exclusive, new GetBranchAnonymousResponsesPaginationQuery { DateBasis = basis, IsScored = true }));
            Assert.Equal(inside, Assert.Single(Assert.Single(anonymousList).CustomInputsPreview).DateValue);
            var detail = Assert.Single(await Repo<AnonymousSurveyResponseCustomInputValue>().ListAsync(
                new GetAnonymousSurveyResponseCustomInputValuesDetailsSpec(anonymousInside)));
            Assert.Equal((TemplateCustomInputType.Date, "Date", inside), (detail.Type, detail.TypeName, detail.DateValue));

            var invalid = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE [SurveyResponseCustomInputValue] SET [DateValue] = {inside} WHERE [Id] = {legacyValueId}
                """));
            Assert.Contains("CK_SRCIV_Date_Value", invalid.Message);
            var duplicate = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE [TemplateCustomInput] SET [SystemFieldKey] = 1 WHERE [Id] = {ordinaryId}
                """));
            Assert.Contains("IX_TemplateCustomInput_TemplateId_SystemFieldKey", duplicate.Message);

            PlatformWriteReadRepository<TEntity> Repo<TEntity>() where TEntity : class
                => new(new PlatformWriteReadDbContextResolver(db));

            async Task<Guid> InsertNormal(Guid templateId, Guid inputId, Guid operatorId, DateTime submitted, DateOnly visit, decimal percentage)
            {
                var id = Guid.NewGuid();
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO [SurveyResponse] ([Id], [OperatorId], [TemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage], [CreatedByApplicationUserId])
                    VALUES ({id}, {operatorId}, {templateId}, {submitted}, {submitted}, 3, 5, {percentage}, {actor});
                    INSERT INTO [SurveyResponseCustomInputValue] ([Id], [SurveyResponseId], [TemplateCustomInputId], [NameSnapshot], [TypeSnapshot], [DateValue], [CreatedOnUtc])
                    VALUES (NEWID(), {id}, {inputId}, N'Visit Time', 3, {visit}, {submitted});
                    """);
                return id;
            }

            async Task<Guid> InsertAnonymous(Guid templateId, Guid inputId, DateTime submitted, DateOnly visit, decimal? percentage)
            {
                var id = Guid.NewGuid();
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO [AnonymousSurveyResponse] ([Id], [AnonymousTemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage])
                    VALUES ({id}, {templateId}, {submitted}, {submitted}, 0, {(percentage.HasValue ? 5 : 0)}, {percentage ?? 0});
                    INSERT INTO [AnonymousSurveyResponseCustomInputValue] ([Id], [AnonymousSurveyResponseId], [AnonymousTemplateCustomInputId], [NameSnapshot], [TypeSnapshot], [DateValue], [CreatedOnUtc])
                    VALUES (NEWID(), {id}, {inputId}, N'Visit Time', 3, {visit}, {submitted});
                    """);
                return id;
            }
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private sealed class BranchScope(Guid branchId, Guid actor) : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result<CurrentBranchScope>.Ok(new CurrentBranchScope
            {
                ApplicationUserId = actor, BranchId = branchId, ActorType = UserType.BranchAdmin
            }));
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
