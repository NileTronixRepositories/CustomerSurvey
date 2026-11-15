using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Reports;
using CustomerSurvey.infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed partial class SurveyHistorySqlTests
{
    [SqlHistoryFact]
    public async Task Rating_submissions_validate_shape_and_visibility_and_persist_raw_answers_and_normalized_scores()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.EnsureCreatedAsync();

        var seed = await SeedTemplatesAsync(db);
        var star = seed.Questions[0];
        var actor = seed.User.Id;
        var globalGroup = QuestionGroup.CreateGlobal("Global ratings", null, actor);
        var root = Question.CreateGlobal(globalGroup.Id, "Recommendation", null, QuestionType.Rating1To10, actor);
        var child = Question.Create(seed.Branch.Id, star.GroupId, "Follow-up rating", null, QuestionType.Rating1To10, actor);
        var optional = Question.Create(seed.Branch.Id, star.GroupId, "Optional rating", null, QuestionType.Rating1To10, actor);
        var questions = new[] { star, root, child, optional };
        var normalLinks = questions.Select((q, i) =>
            TemplateQuestion.Create(seed.Authorized.Id, q.Id, i + 1, actor, isRequired: i != 3)).ToArray();
        var anonLinks = questions.Select((q, i) =>
            AnonymousTemplateQuestion.Create(seed.Anonymous.Id, q.Id, i + 1, actor, isRequired: i != 3)).ToArray();
        db.AddRange(globalGroup, root, child, optional, OperatorTemplate.Create(seed.Operator.Id, seed.Authorized.Id, actor));
        db.AddRange(normalLinks);
        db.AddRange(anonLinks);
        db.Add(TemplateQuestionCondition.CreateForStarRating(seed.Authorized.Id, normalLinks[0].Id, normalLinks[2].Id, 4, 1, actor));
        db.Add(AnonymousTemplateQuestionCondition.CreateForStarRating(seed.Anonymous.Id, anonLinks[0].Id, anonLinks[2].Id, 4, 1, actor));
        await db.SaveChangesAsync();

        var operatorHandler = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            new UnusedSubmissionMedia(), new TestCurrentUser(actor), store.Unit, new FlagPermissions(store.Read<UserRole>()));
        var anonymousHandler = new SubmitAnonymousTemplateResponseCommandHandler(store.Read<AnonymousTemplate>(),
            store.Read<AnonymousTemplateCustomInput>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>(), store.Write<AnonymousSurveyResponse>(),
            new UnusedSubmissionMedia(), store.Unit);

        var stars = new RatingSubmissionAnswer(0, Star: 4);
        var rootRating = new RatingSubmissionAnswer(1, Rating: 9);
        var childRating = new RatingSubmissionAnswer(2, Rating: 1);
        using var media = new MemoryStream([0]);
        var file = new FormFile(media, 0, 1, "media", "test");
        foreach (var invalid in new[]
        {
            rootRating with { Rating = null },
            rootRating with { Rating = 0 },
            rootRating with { Rating = 11 },
            rootRating with { Rating = -1 },
            rootRating with { Star = 5 },
            rootRating with { Option = Guid.NewGuid() },
            rootRating with { Smile = 5 },
            rootRating with { Text = "Mixed answer" },
            rootRating with { Image = file },
            rootRating with { Voice = file }
        })
        {
            await ExpectErrorAsync([stars, invalid, childRating], "Rating1To10ValueInvalid", "Rating1To10AnswerInvalid");
        }

        await ExpectErrorAsync([stars with { Rating = 9 }, rootRating, childRating], "StarRatingValueInvalid", "StarRatingAnswerInvalid");
        await ExpectErrorAsync([stars with { Star = 5 }, rootRating, childRating], "HiddenQuestionAnswerNotAllowed", "HiddenQuestionAnswerNotAllowed");
        await ExpectErrorAsync([stars, rootRating], "VisibleQuestionsRequired", "VisibleQuestionMissingAnswer");
        await ExpectErrorAsync([stars, rootRating, childRating, new(-1, Rating: 9)], "QuestionNotInTemplate", "AnswerQuestionNotFound");
        var duplicateAnswers = new[] { stars, rootRating, rootRating, childRating };
        Assert.Equal("SurveyResponses.Submit.DuplicateQuestionAnswers",
            Assert.Single((await operatorHandler.Handle(OperatorCommand(duplicateAnswers, null), default)).Errors).Code);
        Assert.False(new SubmitAnonymousTemplateResponseCommandValidator().Validate(AnonymousCommand(duplicateAnswers)).IsValid);

        var before = DateTime.UtcNow;
        var normal = await operatorHandler.Handle(OperatorCommand([stars, rootRating, childRating], 225), default);
        var anonymous = await anonymousHandler.Handle(AnonymousCommand([stars, rootRating, childRating]), default);
        Assert.True(normal.IsSuccess, string.Join(", ", normal.Errors));
        Assert.True(anonymous.IsSuccess, string.Join(", ", anonymous.Errors));
        Assert.Equal((9, 10, 90m), (normal.Value.ActualScore, normal.Value.MaxScore, normal.Value.ScorePercentage));
        Assert.Equal((9, 10, 90m), (anonymous.Value.ActualScore, anonymous.Value.MaxScore, anonymous.Value.ScorePercentage));
        Assert.InRange(normal.Value.SubmittedOnUtc, before, DateTime.UtcNow);

        var normalId = normal.Value.SurveyResponseId;
        var anonymousId = anonymous.Value.AnonymousSurveyResponseId;
        var saved = await db.Set<SurveyResponse>().AsNoTracking().SingleAsync(x => x.Id == normalId);
        Assert.Equal(225, saved.AnswerDurationSeconds);
        Assert.Equal(normal.Value.SubmittedOnUtc, saved.SubmittedOnUtc);
        var snapshots = await db.Set<ResponseSnapshot>().AsNoTracking().Where(x => x.ResponseId == normalId || x.ResponseId == anonymousId).ToArrayAsync();
        Assert.Equal(2, snapshots.Length);
        Assert.All(snapshots, snapshot =>
        {
            Assert.Equal(2, snapshot.SchemaVersion);
            var document = snapshot.Read();
            var rating = Assert.Single(document.Questions, q => q.QuestionId == root.Id);
            Assert.Equal(9, rating.Rating1To10Value);
            Assert.Equal(5, SnapshotScoring.Value(rating));
            Assert.Empty(rating.Options);
            Assert.Contains(document.Questions, q => q.QuestionId == optional.Id && q.IsVisible && !q.HasAnswer);
            Assert.Equal((90m, 60m, 66.67m), (snapshot.RootScorePercentage, snapshot.LowestScorePercentage, snapshot.AllScorePercentage));
        });

        var normalAnswers = await store.Read<SurveyAnswer>().ListAsync(new GetBranchSurveyResponseAnswersSpec(normalId), default);
        var anonAnswers = await store.Read<AnonymousSurveyAnswer>().ListAsync(new GetAnonymousSurveyResponseAnswersDetailsSpec(anonymousId), default);
        var normalDetail = Assert.Single(normalAnswers, x => x.QuestionId == root.Id);
        Assert.Equal(QuestionType.Rating1To10, normalDetail.QuestionType);
        Assert.Equal(9, normalDetail.Rating1To10Value);
        Assert.Equal(9, Assert.Single(anonAnswers, x => x.QuestionId == root.Id).Rating1To10Value);

        using var services = new ServiceCollection()
            .AddSingleton<IDbContextResolver<PlatformWriteReadMarker>>(new PlatformWriteReadDbContextResolver(db))
            .AddTransient(typeof(IWriteReadRepository<>), typeof(PlatformWriteReadRepository<>))
            .AddSingleton<ISurveyReportScoringService>(new SurveyReportScoringService())
            .AddSingleton<IPdfService>(new UnusedPdfService())
            .AddTransient<ITemplateAvailabilityReader, TemplateAvailabilityReader>()
            .BuildServiceProvider();
        var reportService = ActivatorUtilities.CreateInstance<BranchTemplatesPdfReportService>(services);
        var report = await reportService.BuildReportModelAsync(new BranchTemplatesPdfReportRequest
        {
            BranchId = seed.Branch.Id, TemplateId = seed.Authorized.Id, TemplateKind = ReportTemplateKind.Normal,
            GeneratedByApplicationUserId = actor, GeneratedByName = "Admin",
            FromDate = DateOnly.FromDateTime(before), ToDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IncludeResponseDetails = true
        }, default);
        Assert.True(report.IsSuccess, string.Join(", ", report.Errors));
        var reportRating = Assert.Single(Assert.Single(report.Value.Responses).Answers, x => x.QuestionId == root.Id);
        Assert.Equal("9", reportRating.DisplayValue);
        Assert.Equal(5m, reportRating.ScoreValue);
        Assert.True(reportRating.IncludedInScore);
        Assert.Equal(90m, report.Value.ExecutiveSummary.AverageScorePercentage);

        foreach (var duration in new int?[] { 0, null })
        {
            RatingSubmissionAnswer[] answers = [stars with { Star = 5 }, rootRating with { Rating = 10 }];
            var result = await operatorHandler.Handle(OperatorCommand(answers, duration), default);
            Assert.True(result.IsSuccess, string.Join(", ", result.Errors));
            Assert.Equal((10, 10, 100m), (result.Value.ActualScore, result.Value.MaxScore, result.Value.ScorePercentage));
            Assert.Equal(duration, await db.Set<SurveyResponse>().Where(x => x.Id == result.Value.SurveyResponseId)
                .Select(x => x.AnswerDurationSeconds).SingleAsync());
            var publicResult = await anonymousHandler.Handle(AnonymousCommand(answers), default);
            Assert.True(publicResult.IsSuccess, string.Join(", ", publicResult.Errors));
            Assert.Equal((10, 10, 100m), (publicResult.Value.ActualScore, publicResult.Value.MaxScore, publicResult.Value.ScorePercentage));
        }

        async Task ExpectErrorAsync(RatingSubmissionAnswer[] answers, string operatorCode, string anonymousCode)
        {
            var normalResult = await operatorHandler.Handle(OperatorCommand(answers, null), default);
            var anonymousResult = await anonymousHandler.Handle(AnonymousCommand(answers), default);
            Assert.Equal("SurveyResponses.Submit." + operatorCode, Assert.Single(normalResult.Errors).Code);
            Assert.Equal("AnonTemplates.Submit." + anonymousCode, Assert.Single(anonymousResult.Errors).Code);
        }

        SubmitOperatorTemplateResponseCommand OperatorCommand(RatingSubmissionAnswer[] answers, int? duration) => new()
        {
            TemplateId = seed.Authorized.Id,
            AnswerDurationSeconds = duration,
            Answers = answers.Select(x => new SubmitOperatorTemplateAnswerCommandItem
            {
                QuestionId = x.Index < 0 ? seed.Questions[1].Id : questions[x.Index].Id,
                Rating1To10Value = x.Rating, StarRatingValue = x.Star, SmileValue = x.Smile,
                SelectedQuestionOptionId = x.Option, TextAnswer = x.Text,
                ImageFile = x.Image, VoiceFile = x.Voice
            }).ToArray()
        };

        SubmitAnonymousTemplateResponseCommand AnonymousCommand(RatingSubmissionAnswer[] answers) => new()
        {
            AnonymousTemplateId = seed.Anonymous.Id,
            Answers = answers.Select(x => new SubmitAnonymousTemplateAnswerCommandItem
            {
                AnonymousTemplateQuestionId = x.Index < 0 ? normalLinks[1].Id : anonLinks[x.Index].Id,
                Rating1To10Value = x.Rating, StarRatingValue = x.Star, SmileValue = x.Smile,
                SelectedQuestionOptionId = x.Option, TextAnswer = x.Text,
                ImageFile = x.Image, VoiceFileName = x.Voice?.FileName
            }).ToArray()
        };
    }

    [SqlHistoryFact]
    public async Task Dashboard_averages_recorded_durations_using_department_template_and_submission_date_filters()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.GetService<IMigrator>().MigrateAsync("20261112120000_AddTemplateQuestionRequiredness");
        var seed = await SeedTemplatesAsync(db);
        var actor = seed.User.Id;
        var templateId = seed.Authorized.Id;
        var operatorId = seed.Operator.Id;
        var submitted = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
        var legacyId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO [SurveyResponse] ([Id], [OperatorId], [TemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage], [CreatedByApplicationUserId])
            VALUES ({legacyId}, {operatorId}, {templateId}, {submitted}, {submitted}, 3, 5, 60, {actor});
            """);

        await db.Database.MigrateAsync();
        var legacy = await db.Set<SurveyResponse>().AsNoTracking().SingleAsync(x => x.Id == legacyId);
        Assert.Null(legacy.AnswerDurationSeconds);
        Assert.Equal((3, 5, 60m), (legacy.ActualScore, legacy.MaxScore, legacy.ScorePercentage));

        var otherTemplate = Template.Create(seed.Branch.Id, "Other template", null, null, submitted.AddDays(-1), null, actor);
        var otherDepartment = Department.Create("Other department", null, actor);
        var adminUser = ApplicationUser.Create("dashboard-admin", "admin@example.test", "Department admin", null, null, "unused", UserType.DepartmentAdmin, actor);
        var admin = DepartmentAdmin.Create(adminUser.Id, seed.Operator.DepartmentId, actor);
        db.AddRange(otherTemplate, otherDepartment, adminUser, admin);
        var noDuration = AddOperator("No duration", seed.Operator.DepartmentId);
        var noResponses = AddOperator("No responses", seed.Operator.DepartmentId);
        var outsideDepartment = AddOperator("Other operator", otherDepartment.Id);
        await db.SaveChangesAsync();

        await InsertResponse(operatorId, templateId, submitted, 0);
        await InsertResponse(operatorId, templateId, submitted, 100);
        await InsertResponse(operatorId, templateId, submitted.AddHours(1), 100);
        await InsertResponse(operatorId, otherTemplate.Id, submitted, 900);
        await InsertResponse(operatorId, templateId, submitted.AddDays(-1), 900);
        await InsertResponse(operatorId, templateId, submitted.AddDays(1), 900);
        await InsertResponse(outsideDepartment.Id, templateId, submitted, 900);
        await InsertResponse(noDuration.Id, templateId, submitted, null);

        var handler = new GetDepartmentDashboardQueryHandler(store.Read<DepartmentAdmin>(), store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<Template>(), store.Read<SurveyResponse>(), store.Read<SurveyAnswer>(),
            store.Read<SurveyResponseCustomInputValue>(), new TestCurrentUser(adminUser.Id));
        var result = await handler.Handle(new GetDepartmentDashboardQuery
        {
            From = DateOnly.FromDateTime(submitted),
            To = DateOnly.FromDateTime(submitted),
            TemplateId = templateId
        }, default);

        Assert.True(result.IsSuccess, string.Join(", ", result.Errors));
        var operators = result.Value.OperatorPerformance;
        var performance = Assert.Single(operators, x => x.OperatorId == operatorId);
        Assert.Equal((4, 4, 75m), (performance.ResponsesCount, performance.ScoredResponsesCount, performance.AverageScorePercentage));
        Assert.Equal(66.67m, performance.AverageAnswerDurationSeconds);
        Assert.Equal(submitted.AddHours(1), performance.LastResponseOnUtc);
        Assert.True(performance.IsActive);
        Assert.Equal(seed.User.NameEn, performance.OperatorNameEn);
        var missing = Assert.Single(operators, x => x.OperatorId == noDuration.Id);
        Assert.Equal(1, missing.ResponsesCount);
        Assert.Null(missing.AverageAnswerDurationSeconds);
        var empty = Assert.Single(operators, x => x.OperatorId == noResponses.Id);
        Assert.Equal(0, empty.ResponsesCount);
        Assert.Null(empty.AverageAnswerDurationSeconds);
        Assert.DoesNotContain(operators, x => x.OperatorId == outsideDepartment.Id);

        Operator AddOperator(string name, Guid departmentId)
        {
            var username = Guid.NewGuid().ToString("N");
            var user = ApplicationUser.Create(username, $"{username}@example.test", name, null, null, "unused", UserType.Operator, actor);
            var op = Operator.Create(user.Id, departmentId, actor);
            db.AddRange(user, op);
            return op;
        }

        Task<int> InsertResponse(Guid op, Guid template, DateTime time, int? duration) =>
            db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [SurveyResponse] ([Id], [OperatorId], [TemplateId], [SubmittedOnUtc], [CreatedOnUtc], [ActualScore], [MaxScore], [ScorePercentage], [CreatedByApplicationUserId], [AnswerDurationSeconds])
                VALUES (NEWID(), {op}, {template}, {time}, {time}, 4, 5, 80, {actor}, {duration});
                """);
    }

    private sealed record RatingSubmissionAnswer(int Index, int? Rating = null, int? Star = null,
        Guid? Option = null, int? Smile = null, string? Text = null, IFormFile? Image = null, IFormFile? Voice = null);
}
