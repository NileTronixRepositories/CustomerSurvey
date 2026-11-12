using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Api.Controllers;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Reports;
using CustomerSurvey.infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed partial class SurveyHistorySqlTests
{
    [SqlHistoryFact]
    public async Task Requiredness_migration_and_assignment_updates_preserve_existing_rows_and_omitted_values()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.GetService<IMigrator>().MigrateAsync("20261111143557_AddAuthorizedSurveyResponseFlags");
        var seed = await SeedTemplatesAsync(db);
        var globalGroup = QuestionGroup.CreateGlobal("Global", null, seed.User.Id);
        var global = Question.CreateGlobal(globalGroup.Id, "Global rating", null, QuestionType.StarRating, seed.User.Id);
        db.AddRange(globalGroup, global);
        await db.SaveChangesAsync();
        var existingQuestion = seed.Questions[0].Id;
        var authorizedId = Guid.NewGuid();
        var anonymousId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO [TemplateQuestion] ([Id], [TemplateId], [QuestionId], [Order], [CreatedByApplicationUserId], [CreatedOnUtc])
            VALUES ({authorizedId}, {seed.Authorized.Id}, {existingQuestion}, 1, {seed.User.Id}, {now});
            INSERT INTO [AnonymousTemplateQuestion] ([Id], [AnonymousTemplateId], [QuestionId], [Order], [CreatedByApplicationUserId], [CreatedOnUtc])
            VALUES ({anonymousId}, {seed.Anonymous.Id}, {existingQuestion}, 1, {seed.User.Id}, {now});
            """);

        await db.Database.MigrateAsync();
        var authorized = await db.Set<TemplateQuestion>().SingleAsync();
        var anonymous = await db.Set<AnonymousTemplateQuestion>().SingleAsync();
        Assert.True(authorized.IsRequired);
        Assert.True(anonymous.IsRequired);
        Assert.Equal(authorizedId, authorized.Id);
        Assert.Equal(anonymousId, anonymous.Id);
        var scope = new AssignmentScope(seed.Branch.Id, seed.User.Id);
        var authorizedHandler = new AssignQuestionsToTemplateCommandHandler(
            store.Read<Template>(), store.Read<Question>(), store.Read<TemplateQuestion>(), store.Write<TemplateQuestion>(),
            store.Read<TemplateQuestionCondition>(), store.Write<TemplateQuestionCondition>(), scope, new TestCurrentUser(seed.User.Id), store.Unit);
        var anonymousHandler = new AssignQuestionsToAnonymousTemplateCommandHandler(
            store.Read<AnonymousTemplate>(), store.Read<AnonymousTemplateQuestion>(), store.Write<AnonymousTemplateQuestion>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Write<AnonymousTemplateQuestionCondition>(),
            store.Read<Question>(), store.Read<SuperAdmin>(), scope, new TestCurrentUser(seed.User.Id), store.Unit);

        await AssignAsync(false, includeGlobal: true);
        await AssignAsync(null, includeGlobal: true);
        var authorizedAssignments = await db.Set<TemplateQuestion>().OrderBy(x => x.Order).AsNoTracking().ToArrayAsync();
        var anonymousAssignments = await db.Set<AnonymousTemplateQuestion>().OrderBy(x => x.Order).AsNoTracking().ToArrayAsync();
        Assert.Equal(new[] { global.Id, existingQuestion }, authorizedAssignments.Select(x => x.QuestionId));
        Assert.Equal(new[] { global.Id, existingQuestion }, anonymousAssignments.Select(x => x.QuestionId));
        Assert.All(authorizedAssignments, x => Assert.False(x.IsRequired));
        Assert.All(anonymousAssignments, x => Assert.False(x.IsRequired));
        Assert.Equal(authorizedId, authorizedAssignments[1].Id);
        Assert.Equal(anonymousId, anonymousAssignments[1].Id);
        Assert.Equal(now, authorizedAssignments[1].CreatedOnUtc);
        Assert.Equal(now, anonymousAssignments[1].CreatedOnUtc);

        db.AddRange(
            TemplateQuestionCondition.CreateForStarRating(seed.Authorized.Id, authorizedId, authorizedAssignments[0].Id, 4, 1, seed.User.Id),
            AnonymousTemplateQuestionCondition.CreateForStarRating(seed.Anonymous.Id, anonymousId, anonymousAssignments[0].Id, 4, 1, seed.User.Id));
        await db.SaveChangesAsync();
        await AssignAsync(true, includeGlobal: false);
        Assert.True((await db.Set<TemplateQuestion>().AsNoTracking().SingleAsync()).IsRequired);
        Assert.True((await db.Set<AnonymousTemplateQuestion>().AsNoTracking().SingleAsync()).IsRequired);
        Assert.Empty(await db.Set<TemplateQuestionCondition>().ToListAsync());
        Assert.Empty(await db.Set<AnonymousTemplateQuestionCondition>().ToListAsync());

        async Task AssignAsync(bool? required, bool includeGlobal)
        {
            var normal = await authorizedHandler.Handle(new AssignQuestionsToTemplateCommand
            {
                TemplateId = seed.Authorized.Id,
                QuestionIds = includeGlobal ? [global.Id, existingQuestion] : [existingQuestion],
                QuestionRequirements = required is null ? [] : includeGlobal
                    ? [new(existingQuestion, required), new(global.Id, required)]
                    : [new(existingQuestion, required)]
            }, default);
            Assert.True(normal.IsSuccess, string.Join(", ", normal.Errors));
            Assert.All(normal.Value.Questions, x => Assert.Equal(required ?? false, x.IsRequired));

            var publicResult = await anonymousHandler.Handle(new AssignQuestionsToAnonymousTemplateCommand
            {
                AnonymousTemplateId = seed.Anonymous.Id,
                Questions = includeGlobal
                    ? [new() { QuestionId = global.Id, Order = 1, IsRequired = required },
                        new() { QuestionId = existingQuestion, Order = 2, IsRequired = required }]
                    : [new() { QuestionId = existingQuestion, Order = 1, IsRequired = required }]
            }, default);
            Assert.True(publicResult.IsSuccess, string.Join(", ", publicResult.Errors));
            Assert.All(publicResult.Value.Questions, x => Assert.Equal(required ?? false, x.IsRequired));
        }
    }

    [SqlHistoryFact]
    public async Task Optional_submissions_validate_nested_visibility_and_preserve_scoring_and_report_counts()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedTemplatesAsync(db);
        var (authorized, anonymous, _, user, op, questions) = seed;
        var normalLinks = questions.Select((q, i) =>
            TemplateQuestion.Create(authorized.Id, q.Id, i + 1, user.Id, isRequired: i is 0 or 2 or 3)).ToArray();
        var anonLinks = questions.Select((q, i) =>
            AnonymousTemplateQuestion.Create(anonymous.Id, q.Id, i + 1, user.Id, isRequired: i is 0 or 2 or 3)).ToArray();
        var normalInput = TemplateCustomInput.Create(authorized.Id, "Reference", null, TemplateCustomInputType.String,
            true, null, null, null, null, null, 2, user.Id);
        var anonInput = AnonymousTemplateCustomInput.Create(anonymous.Id, "Reference", null, TemplateCustomInputType.String,
            true, null, null, null, null, null, 2, user.Id);
        db.AddRange(normalLinks);
        db.AddRange(anonLinks);
        db.AddRange(normalInput, anonInput, OperatorTemplate.Create(op.Id, authorized.Id, user.Id));
        foreach (var (parent, child, rating, order) in new[] { (0, 1, 4, 1), (0, 2, 4, 2), (1, 3, 2, 1) })
        {
            db.Add(TemplateQuestionCondition.CreateForStarRating(authorized.Id, normalLinks[parent].Id, normalLinks[child].Id, rating, order, user.Id));
            db.Add(AnonymousTemplateQuestionCondition.CreateForStarRating(anonymous.Id, anonLinks[parent].Id, anonLinks[child].Id, rating, order, user.Id));
        }
        await db.SaveChangesAsync();

        var operatorHandler = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            new UnusedSubmissionMedia(), new TestCurrentUser(user.Id), store.Unit, new FlagPermissions(store.Read<UserRole>()));
        var anonymousHandler = new SubmitAnonymousTemplateResponseCommandHandler(store.Read<AnonymousTemplate>(),
            store.Read<AnonymousTemplateCustomInput>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>(), store.Write<AnonymousSurveyResponse>(),
            new UnusedSubmissionMedia(), store.Unit);

        await ExpectErrorAsync("CustomInputValueRequired", "RequiredCustomInputMissing", [], includeInput: false);
        await ExpectErrorAsync("VisibleQuestionsRequired", "VisibleQuestionMissingAnswer", []);
        normalLinks[0].SetRequired(false);
        anonLinks[0].SetRequired(false);
        await db.SaveChangesAsync();
        await ExpectErrorAsync("HiddenQuestionAnswerNotAllowed", "HiddenQuestionAnswerNotAllowed", [new(1, 2)]);
        await ExpectErrorAsync("VisibleQuestionsRequired", "VisibleQuestionMissingAnswer", [new(0, 4)]);
        await ExpectErrorAsync("StarRatingValueInvalid", "StarRatingAnswerInvalid", [new(0, 4), new(2, 5), new(1, 6)]);
        await ExpectErrorAsync("StarRatingValueInvalid", "StarRatingAnswerInvalid", [new(0, 4), new(2, 5), new(1)]);
        await ExpectErrorAsync("SmilesValueInvalid", "SmilesAnswerInvalid", [new(6, Smile: 6)]);
        await ExpectErrorAsync("VisibleQuestionsRequired", "VisibleQuestionMissingAnswer", [new(0, 4), new(2, 5), new(1, 2)]);

        await ExpectScoreAsync([], 0, 0, 0);
        await ExpectScoreAsync([new(5, Text: "Comment")], 0, 0, 0);
        var dashboard = new GetAnonymousTemplateDashboardQueryHandler(store.Read<Branch>(), store.Read<AnonymousTemplate>(),
            store.Read<AnonymousSurveyResponse>(), store.Read<AnonymousSurveyAnswer>(), store.Read<AnonymousSurveyResponseCustomInputValue>(),
            new AssignmentScope(seed.Branch.Id, user.Id), new TestCurrentUser(user.Id));
        var dashboardQuery = new GetAnonymousTemplateDashboardQuery { AnonymousTemplateId = anonymous.Id, CriticalScoreThreshold = 100 };
        var noScoreData = (await dashboard.Handle(dashboardQuery, default)).Value;
        Assert.Equal(2, noScoreData.Summary.TotalResponses);
        Assert.False(noScoreData.Summary.HasScoreData);
        Assert.Equal("NoData", Assert.Single(noScoreData.AnonymousTemplatePerformance).RiskLevel);
        Assert.Empty(noScoreData.CriticalResponses);

        var mixed = await ExpectScoreAsync([new(0, 4), new(2, 5), new(4, 5)], 9, 10, 90);
        foreach (var snapshot in await db.Set<ResponseSnapshot>().Where(x => mixed.Contains(x.ResponseId)).ToArrayAsync())
        {
            var document = snapshot.Read();
            Assert.Equal(2, snapshot.SchemaVersion);
            Assert.Equal(2, document.SchemaVersion);
            Assert.True(document.IsHistoricallyVerified);
            var skip = Assert.Single(document.Questions, x => x.QuestionId == questions[1].Id);
            Assert.True(skip.IsVisible);
            Assert.False(skip.IsRequired);
            Assert.False(skip.HasAnswer);
            Assert.True(document.Questions.Single(x => x.QuestionId == questions[2].Id).IsRequired);
            Assert.DoesNotContain(document.Questions, x => x.QuestionId == questions[3].Id);
            Assert.Equal(90m, snapshot.RootScorePercentage);
            Assert.Equal(100m, snapshot.LowestScorePercentage);
            Assert.Equal(93.33m, snapshot.AllScorePercentage);
        }

        var nested = await ExpectScoreAsync([new(0, 4), new(2, 5), new(1, 2), new(3, 1)], 4, 5, 80);
        Assert.All(await db.Set<ResponseSnapshot>().Where(x => nested.Contains(x.ResponseId)).ToArrayAsync(),
            snapshot => Assert.Equal(20m, snapshot.LowestScorePercentage));
        normalLinks[3].SetRequired(false);
        anonLinks[3].SetRequired(false);
        await db.SaveChangesAsync();
        var optionalNested = await ExpectScoreAsync([new(0, 4), new(2, 5), new(1, 2)], 4, 5, 80);
        Assert.All(await db.Set<ResponseSnapshot>().Where(x => optionalNested.Contains(x.ResponseId)).ToArrayAsync(),
            snapshot => Assert.Equal(40m, snapshot.LowestScorePercentage));
        var dashboardData = (await dashboard.Handle(dashboardQuery, default)).Value;
        Assert.Equal((5, 3, 2, 0), (dashboardData.Summary.TotalResponses, dashboardData.Summary.ScoredResponses,
            dashboardData.Summary.UnscoredResponses, dashboardData.Summary.UnhappyResponses));
        Assert.Equal(83.33m, dashboardData.Summary.AverageScorePercentage);
        Assert.Equal(3, dashboardData.CriticalResponses.Count);
        normalLinks[1].SetRequired(true);
        anonLinks[1].SetRequired(true);
        await db.SaveChangesAsync();
        Assert.All(await db.Set<ResponseSnapshot>().AsNoTracking().Where(x => mixed.Contains(x.ResponseId)).ToArrayAsync(),
            snapshot => Assert.False(snapshot.Read().Questions.Single(x => x.QuestionId == questions[1].Id).IsRequired));
        var immutable = await db.Set<ResponseSnapshot>().AsNoTracking().FirstAsync(x => mixed.Contains(x.ResponseId));
        var authorizedResponse = await db.Set<SurveyResponse>().Include(x => x.Snapshot).SingleAsync(x => x.Id == mixed[0]);
        Assert.Throws<InvalidOperationException>(() => authorizedResponse.CaptureSnapshot(immutable.Read()));

        foreach (var mode in Enum.GetValues<ScoreCalculationMode>())
        {
            var repository = store.Read<AnonymousSurveyResponse>();
            var page = await repository.ListWithCountAsync(PageSpec());
            Assert.Equal(5, page.Count);
            Assert.Equal(2, page.Data.Count(x => !x.IsScored));
            foreach (var filtered in new[] { PageSpec(min: 0), PageSpec(max: 100) })
            {
                var scored = await repository.ListWithCountAsync(filtered);
                Assert.Equal(3, scored.Count);
                Assert.All(scored.Data, x => Assert.True(x.IsScored));
            }

            GetAnonymousTemplateResponsesPaginationSpec PageSpec(decimal? min = null, decimal? max = null) => new(
                new GetAnonymousTemplateResponsesPaginationQuery
                {
                    AnonymousTemplateId = anonymous.Id, ScoreCalculationMode = mode,
                    MinScorePercentage = min, MaxScorePercentage = max
                });
        }

        await VerifyOptionalQuestionReportAsync(store, seed);
        await VerifyAnonymousMultipartAsync(anonymousHandler, anonymous.Id, anonInput.Id);

        async Task ExpectErrorAsync(string operatorCode, string anonymousCode, TestAnswer[] answers, bool includeInput = true)
        {
            var normal = await SubmitOperatorAsync(answers, includeInput);
            var publicResult = await SubmitAnonymousAsync(answers, includeInput);
            Assert.Equal("SurveyResponses.Submit." + operatorCode, Assert.Single(normal.Errors).Code);
            Assert.Equal("AnonTemplates.Submit." + anonymousCode, Assert.Single(publicResult.Errors).Code);
        }

        async Task<Guid[]> ExpectScoreAsync(TestAnswer[] answers, int actual, int max, decimal percentage)
        {
            var normal = await SubmitOperatorAsync(answers, true);
            var publicResult = await SubmitAnonymousAsync(answers, true);
            Assert.True(normal.IsSuccess, string.Join(", ", normal.Errors));
            Assert.True(publicResult.IsSuccess, string.Join(", ", publicResult.Errors));
            Assert.Equal((actual, max, percentage, max > 0, answers.Length),
                (normal.Value.ActualScore, normal.Value.MaxScore, normal.Value.ScorePercentage, normal.Value.IsScored, normal.Value.AnswersCount));
            Assert.Equal((actual, max, percentage, max > 0, answers.Length),
                (publicResult.Value.ActualScore, publicResult.Value.MaxScore, publicResult.Value.ScorePercentage, publicResult.Value.IsScored, publicResult.Value.AnswersCount));
            return [normal.Value.SurveyResponseId, publicResult.Value.AnonymousSurveyResponseId];
        }

        Task<Result<SubmitOperatorTemplateResponseResponse>> SubmitOperatorAsync(TestAnswer[] answers, bool includeInput) =>
            operatorHandler.Handle(new SubmitOperatorTemplateResponseCommand
            {
                TemplateId = authorized.Id,
                CustomInputs = includeInput ? [new() { CustomInputId = normalInput.Id, Value = "TEST" }] : [],
                Answers = answers.Select(x => new SubmitOperatorTemplateAnswerCommandItem
                {
                    QuestionId = questions[x.QuestionIndex].Id, StarRatingValue = x.Rating, SmileValue = x.Smile, TextAnswer = x.Text
                }).ToArray()
            }, default);

        Task<Result<SubmitAnonymousTemplateResponseResult>> SubmitAnonymousAsync(TestAnswer[] answers, bool includeInput) =>
            anonymousHandler.Handle(new SubmitAnonymousTemplateResponseCommand
            {
                AnonymousTemplateId = anonymous.Id,
                CustomInputValues = includeInput ? [new() { CustomInputId = anonInput.Id, StringValue = "TEST" }] : [],
                Answers = answers.Select(x => new SubmitAnonymousTemplateAnswerCommandItem
                {
                    AnonymousTemplateQuestionId = anonLinks[x.QuestionIndex].Id, StarRatingValue = x.Rating, SmileValue = x.Smile, TextAnswer = x.Text
                }).ToArray()
            }, default);
    }

    private static async Task VerifyOptionalQuestionReportAsync(OptionalQuestionStore store, SeededTemplates seed)
    {
        using var services = new ServiceCollection()
            .AddSingleton<IDbContextResolver<PlatformWriteReadMarker>>(new PlatformWriteReadDbContextResolver(store.Db))
            .AddTransient(typeof(IWriteReadRepository<>), typeof(PlatformWriteReadRepository<>))
            .AddSingleton<ISurveyReportScoringService>(new SurveyReportScoringService())
            .AddSingleton<IPdfService>(new UnusedPdfService())
            .AddTransient<ITemplateAvailabilityReader, TemplateAvailabilityReader>()
            .BuildServiceProvider();
        var reportService = ActivatorUtilities.CreateInstance<BranchTemplatesPdfReportService>(services);
        var report = await reportService.BuildReportModelAsync(new BranchTemplatesPdfReportRequest
        {
            BranchId = seed.Branch.Id, TemplateId = seed.Authorized.Id, TemplateKind = ReportTemplateKind.Normal,
            GeneratedByApplicationUserId = seed.User.Id, GeneratedByName = "Admin",
            FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), ToDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ScoreCalculationMode = ScoreCalculationMode.RootQuestions
        }, default);
        Assert.True(report.IsSuccess, string.Join(", ", report.Errors));
        Assert.Equal(5, report.Value.ExecutiveSummary.TotalResponses);
        Assert.Equal(3, report.Value.ExecutiveSummary.ScoredResponsesCount);
        Assert.Equal(83.33m, report.Value.ExecutiveSummary.AverageScorePercentage);
        Assert.Equal(0, report.Value.ExecutiveSummary.UnhappyResponses);
        Assert.Equal(1, report.Value.Questions.Single(x => x.QuestionId == seed.Questions[1].Id).SkippedCount);
        Assert.Equal(1, report.Value.Questions.Single(x => x.QuestionId == seed.Questions[3].Id).SkippedCount);
    }

    private static async Task VerifyAnonymousMultipartAsync(SubmitAnonymousTemplateResponseCommandHandler handler, Guid templateId, Guid customInputId)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(AnonTemplatesController).Assembly);
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<SurveyHistorySqlTests>();
            config.AddOpenBehavior(typeof(BuildingBlock.Application.Behaviors.ValidationBehavior<,>));
        });
        builder.Services.AddSingleton<FluentValidation.IValidator<SubmitAnonymousTemplateResponseCommand>, SubmitAnonymousTemplateResponseCommandValidator>();
        builder.Services.AddSingleton<MediatR.IRequestHandler<SubmitAnonymousTemplateResponseCommand, Result<SubmitAnonymousTemplateResponseResult>>>(handler);
        await using var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();

        using var client = new HttpClient { BaseAddress = new Uri(Assert.Single(app.Urls)) };
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(customInputId.ToString()), "CustomInputValues[0].CustomInputId");
        content.Add(new StringContent("TEST"), "CustomInputValues[0].StringValue");
        using var response = await client.PostAsync($"/api/anon-templates/{templateId}/responses", content);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(0, json.RootElement.GetProperty("answersCount").GetInt32());
        Assert.False(json.RootElement.GetProperty("isScored").GetBoolean());
    }

    private static async Task<SeededTemplates> SeedTemplatesAsync(PlatformWriteDbContext db)
    {
        var actor = Guid.NewGuid();
        var branch = Branch.Create("Optional questions", null, "OPTIONAL", null, actor);
        var user = ApplicationUser.Create("optional", "optional@example.test", "Optional", null, null, "unused", UserType.Operator, actor);
        var department = Department.Create("Support", null, actor);
        var op = Operator.Create(user.Id, department.Id, actor);
        var group = QuestionGroup.Create(branch.Id, "Ratings", null, actor);
        var questionTypes = new[] { QuestionType.StarRating, QuestionType.StarRating, QuestionType.StarRating,
            QuestionType.StarRating, QuestionType.StarRating, QuestionType.FreeText, QuestionType.Smiles };
        var questions = questionTypes.Select((type, i) =>
            Question.Create(branch.Id, group.Id, $"Question {i}", null, type, actor)).ToArray();
        var authorized = Template.Create(branch.Id, "Operator", null, null, DateTime.UtcNow.AddDays(-1), null, user.Id);
        var anonymous = AnonymousTemplate.CreateBranchTemplate(branch.Id, "Anonymous", null, null, DateTime.UtcNow.AddDays(-1), null, user.Id);
        db.AddRange(branch, user, department, op, group, authorized, anonymous);
        db.AddRange(questions);
        await db.SaveChangesAsync();
        return new(authorized, anonymous, branch, user, op, questions);
    }

    private sealed record SeededTemplates(Template Authorized, AnonymousTemplate Anonymous, Branch Branch,
        ApplicationUser User, Operator Operator, Question[] Questions);

    private sealed record TestAnswer(int QuestionIndex, int? Rating = null, string? Text = null, int? Smile = null);

    private sealed class AssignmentScope(Guid branchId, Guid actorId) : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result<CurrentBranchScope>.Ok(new() { BranchId = branchId, ApplicationUserId = actorId, ActorType = UserType.BranchAdmin }));
    }

    private sealed class OptionalQuestionStore : IAsyncDisposable
    {
        public PlatformWriteDbContext Db { get; } = new(new DbContextOptionsBuilder<PlatformWriteDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database=CustomerSurveyOptionalTests_{Guid.NewGuid():N};Integrated Security=true;TrustServerCertificate=true").Options, new Tenant());

        public IUnitOfWork Unit => new PlatformUnitOfWork(new PlatformWriteDbContextResolver(Db));
        public IWriteReadRepository<T> Read<T>() where T : class => new PlatformWriteReadRepository<T>(new PlatformWriteReadDbContextResolver(Db));
        public IWriteRepository<T> Write<T>() where T : class => new PlatformWriteRepository<T>(new PlatformWriteDbContextResolver(Db));

        public async ValueTask DisposeAsync()
        {
            await Db.Database.EnsureDeletedAsync();
            await Db.DisposeAsync();
        }
    }

    private sealed class UnusedSubmissionMedia : IMediaService
    {
        public Task<string> SaveAsync(IFormFile mediaFile, string folderName) => throw new NotSupportedException();
        public Task<List<string>> SaveAsync(List<IFormFile> formFiles, string folderName) => throw new NotSupportedException();
        public Task<string> CopyAsync(string relativePath, string folderName) => throw new NotSupportedException();
        public Task<Stream> GetStream(IFormFile formFile) => throw new NotSupportedException();
        public void Remove(string filePath) => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<string> filePaths) => throw new NotSupportedException();
        public Task<string> SaveVideoAsync(IFormFile mediaFile, string folderName) => throw new NotSupportedException();
    }
}
