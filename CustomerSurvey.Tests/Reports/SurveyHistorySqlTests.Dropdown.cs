using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.Questions.Command.CreateQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.infrastructure.Options;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Reports;
using CustomerSurvey.infrastructure.Repositories;
using CustomerSurvey.infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed partial class SurveyHistorySqlTests
{
    [SqlHistoryFact]
    public async Task Global_dropdown_options_survive_edits_and_are_shared_by_the_assigned_public_survey()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.MigrateAsync();
        var seed = await SeedTemplatesAsync(db);
        var actor = seed.User.Id;
        var user = new TestCurrentUser(actor);
        var group = QuestionGroup.CreateGlobal("Services", null, actor);
        db.AddRange(group, SuperAdmin.Create(actor, actor));
        await db.SaveChangesAsync();

        var create = new CreateGlobalQuestionCommandHandler(store.Read<QuestionGroup>(), store.Write<Question>(),
            store.Write<QuestionOption>(), store.Read<SuperAdmin>(), user, store.Unit);
        var request = new CreateGlobalQuestionCommand
        {
            GroupId = group.Id, Type = QuestionType.Dropdown, TextEn = "Choose a service",
            Options = Enumerable.Range(1, 19).Reverse().Select(i => new CreateGlobalQuestionOptionCommandItem
            {
                TextEn = $"Service {i}", Order = i
            }).ToArray()
        };
        var created = await create.Handle(request, default);
        Assert.True(created.IsSuccess, string.Join(", ", created.Errors));
        Assert.Equal(Enumerable.Range(1, 19), created.Value.Options.Select(x => x.Order));
        var questionId = created.Value.QuestionId;
        var global = await db.Set<Question>().AsNoTracking().SingleAsync(x => x.Id == questionId);
        Assert.Equal((QuestionScope.Global, (Guid?)null, QuestionType.Dropdown), (global.Scope, global.BranchId, global.Type));

        var original = created.Value.Options.ToArray();
        var update = new UpdateGlobalQuestionCommandHandler(store.Read<Question>(), store.Read<QuestionGroup>(),
            CreateQuestionUpdater(store), store.Read<SuperAdmin>(), user);
        var edit = new UpdateGlobalQuestionCommand
        {
            QuestionId = questionId, GroupId = group.Id, Type = QuestionType.Dropdown, TextEn = request.TextEn,
            Options = [
                new() { OptionId = original[0].OptionId, TextEn = "Renamed", TextAr = "خدمة", Order = 2 },
                new() { TextEn = "New", Order = 1 }
            ]
        };
        var edited = await update.Handle(edit, default);
        Assert.True(edited.IsSuccess, string.Join(", ", edited.Errors));
        var added = Assert.Single(edited.Value.Options, x => x.Order == 1);
        Assert.Equal(original[0].OptionId, Assert.Single(edited.Value.Options, x => x.Order == 2).OptionId);

        var restore = new UpdateGlobalQuestionOptionCommandItem
        {
            OptionId = original[1].OptionId, TextEn = original[1].TextEn, Order = 1
        };
        var duplicate = await update.Handle(edit with { Options = [restore, restore with { Order = 3 }] }, default);
        Assert.Equal("GlobalQuestions.Update.OptionIdDuplicated", Assert.Single(duplicate.Errors).Code);
        var foreign = await update.Handle(edit with { Options = [restore with { OptionId = Guid.NewGuid() }] }, default);
        Assert.Equal("GlobalQuestions.Update.OptionNotBelongToQuestion", Assert.Single(foreign.Errors).Code);
        var restored = await update.Handle(edit with { Options = [edit.Options.First(), restore] }, default);
        Assert.True(restored.IsSuccess, string.Join(", ", restored.Errors));
        var persisted = await db.Set<QuestionOption>().AsNoTracking().Where(x => x.QuestionId == questionId).ToArrayAsync();
        Assert.Equal(20, persisted.Length);
        Assert.False(Assert.Single(persisted, x => x.Id == added.OptionId).IsActive);
        Assert.Equal(new[] { original[1].OptionId, original[0].OptionId },
            persisted.Where(x => x.IsActive).OrderBy(x => x.Order).Select(x => x.Id));
        Assert.All(persisted, x => Assert.Equal(0, x.Value));

        var source = AnonymousTemplate.CreateGlobalTemplate("Service catalog", null, null, DateTime.UtcNow.AddDays(-1), null, actor);
        db.AddRange(source, AnonymousTemplateQuestion.Create(source.Id, questionId, 4, actor, isRequired: false));
        await db.SaveChangesAsync();
        var assign = new AssignGlobalAnonymousTemplateToBranchCommandHandler(store.Read<SuperAdmin>(), store.Read<Branch>(),
            store.Read<AnonymousTemplate>(), store.Read<AnonymousTemplateQuestionCondition>(), store.Write<AnonymousTemplate>(),
            store.Write<AnonymousTemplateQuestion>(), store.Write<AnonymousTemplateQuestionCondition>(),
            new PublicSurveyUrlBuilder(Microsoft.Extensions.Options.Options.Create(new PublicSurveyOptions
            {
                AnonymousTemplateBaseUrl = "https://example.test/survey"
            })), new QrCodeGenerator(), new UnusedSubmissionMedia(), user, store.Unit);
        var assignment = await assign.Handle(new()
        {
            GlobalTemplateId = source.Id, BranchId = seed.Branch.Id, ActiveFrom = source.ActiveFrom
        }, default);
        Assert.True(assignment.IsSuccess, string.Join(", ", assignment.Errors));

        var publicQuery = new GetPublicAnonymousTemplateQueryHandler(store.Read<AnonymousTemplate>(),
            store.Read<AnonymousTemplateCustomInput>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>());
        Assert.False((await publicQuery.Handle(new() { AnonymousTemplateId = source.Id }, default)).IsSuccess);
        var templateId = assignment.Value.AnonymousTemplateId;
        var retrieved = await publicQuery.Handle(new() { AnonymousTemplateId = templateId }, default);
        Assert.True(retrieved.IsSuccess, string.Join(", ", retrieved.Errors));
        var dropdown = Assert.Single(retrieved.Value.Questions);
        Assert.Equal((questionId, 9, "Dropdown", 4, false),
            (dropdown.QuestionId, (int)dropdown.Type, dropdown.TypeName, dropdown.Order, dropdown.IsRequired));
        Assert.Collection(dropdown.Options,
            x => Assert.Equal((original[1].OptionId, original[1].TextEn, 1), (x.OptionId, x.TextEn, x.Order)),
            x => Assert.Equal((original[0].OptionId, "Renamed", "خدمة", 2), (x.OptionId, x.TextEn, x.TextAr, x.Order)));

        var submit = CreateDropdownAnonymousHandler(store);
        var omitted = await submit.Handle(new() { AnonymousTemplateId = templateId }, default);
        Assert.True(omitted.IsSuccess, string.Join(", ", omitted.Errors));
        var answered = await submit.Handle(new()
        {
            AnonymousTemplateId = templateId, Answers = [new()
            {
                AnonymousTemplateQuestionId = dropdown.AnonymousTemplateQuestionId, SelectedQuestionOptionId = original[0].OptionId
            }]
        }, default);
        Assert.True(answered.IsSuccess, string.Join(", ", answered.Errors));
        Assert.Equal((0, 0, 0m, false, 1), (answered.Value.ActualScore, answered.Value.MaxScore,
            answered.Value.ScorePercentage, answered.Value.IsScored, answered.Value.AnswersCount));
        var snapshot = await db.Set<ResponseSnapshot>().SingleAsync(x => x.ResponseId == answered.Value.AnonymousSurveyResponseId);
        foreach (var mode in Enum.GetValues<ScoreCalculationMode>())
            Assert.Empty(SnapshotScoring.Tokens(snapshot.Read(), mode));

        var asText = await update.Handle(edit with { Type = QuestionType.FreeText, Options = [] }, default);
        Assert.True(asText.IsSuccess, string.Join(", ", asText.Errors));
        Assert.Empty(asText.Value.Options);
        Assert.False(await db.Set<QuestionOption>().AnyAsync(x => x.QuestionId == questionId && x.IsActive));

        var restoredFromText = await update.Handle(edit with { Options = [restore] }, default);
        Assert.True(restoredFromText.IsSuccess, string.Join(", ", restoredFromText.Errors));
        Assert.Equal(restore.OptionId, Assert.Single(restoredFromText.Value.Options).OptionId);

        var asSingleChoice = await update.Handle(edit with
        {
            Type = QuestionType.SingleChoice, Options = [restore with { Value = 5 }]
        }, default);
        Assert.True(asSingleChoice.IsSuccess, string.Join(", ", asSingleChoice.Errors));
        Assert.Equal(5, Assert.Single(asSingleChoice.Value.Options).Value);

        var backToDropdown = await update.Handle(edit with { Options = [restore] }, default);
        Assert.True(backToDropdown.IsSuccess, string.Join(", ", backToDropdown.Errors));
        var restoredOption = Assert.Single(backToDropdown.Value.Options);
        Assert.Equal((restore.OptionId, 0), ((Guid?)restoredOption.OptionId, restoredOption.Value));
    }

    [SqlHistoryFact]
    public async Task Dropdown_submissions_validate_selection_and_keep_scores_and_historical_labels()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedTemplatesAsync(db);
        var actor = seed.User.Id;
        var groupId = seed.Questions[0].GroupId;
        var scope = new AssignmentScope(seed.Branch.Id, actor);
        var user = new TestCurrentUser(actor);
        var create = new CreateQuestionCommandHandler(store.Read<QuestionGroup>(), store.Write<Question>(),
            store.Write<QuestionOption>(), scope, user, store.Unit);
        var created = await create.Handle(new()
        {
            GroupId = groupId, Type = QuestionType.Dropdown, TextEn = "Choose a specialty",
            Options = [new() { TextEn = "Cardiology", TextAr = "القلب", Order = 1 }, new() { TextEn = "Inactive", Order = 2 }]
        }, default);
        Assert.True(created.IsSuccess, string.Join(", ", created.Errors));
        var dropdownId = created.Value.QuestionId;
        var selectedId = created.Value.Options.First().OptionId;
        var inactiveId = created.Value.Options.Last().OptionId;
        var inactive = await db.Set<QuestionOption>().SingleAsync(x => x.Id == inactiveId);
        inactive.Deactivate();

        var first = Question.Create(seed.Branch.Id, groupId, "First rating", null, QuestionType.SingleChoice, actor);
        var second = Question.Create(seed.Branch.Id, groupId, "Second rating", null, QuestionType.SingleChoice, actor);
        var three = QuestionOption.Create(first.Id, "Three", null, 1, 3, actor);
        var hideChild = QuestionOption.Create(first.Id, "Five", null, 2, 5, actor);
        var five = QuestionOption.Create(second.Id, "Five", null, 1, 5, actor);
        var questionIds = new[] { first.Id, second.Id, dropdownId };
        var authorizedLinks = questionIds.Select((id, i) => TemplateQuestion.Create(seed.Authorized.Id, id, i + 1, actor)).ToArray();
        var anonymousLinks = questionIds.Select((id, i) => AnonymousTemplateQuestion.Create(seed.Anonymous.Id, id, i + 1, actor)).ToArray();
        db.AddRange(first, second, three, hideChild, five, OperatorTemplate.Create(seed.Operator.Id, seed.Authorized.Id, actor));
        db.AddRange(authorizedLinks);
        db.AddRange(anonymousLinks);
        db.Add(TemplateQuestionCondition.CreateForSingleChoice(seed.Authorized.Id, authorizedLinks[0].Id, authorizedLinks[2].Id, three.Id, 1, actor));
        db.Add(AnonymousTemplateQuestionCondition.CreateForSingleChoice(seed.Anonymous.Id, anonymousLinks[0].Id, anonymousLinks[2].Id, three.Id, 1, actor));
        await db.SaveChangesAsync();

        var anonymous = CreateDropdownAnonymousHandler(store);
        var authorized = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(), store.Read<OperatorTemplate>(),
            store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(), store.Read<QuestionOption>(),
            store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(), new UnusedSubmissionMedia(), user,
            store.Unit, new FlagPermissions(store.Read<UserRole>()));
        var anonRequest = new SubmitAnonymousTemplateResponseCommand
        {
            AnonymousTemplateId = seed.Anonymous.Id,
            Answers = anonymousLinks.Zip(new[] { three.Id, five.Id, selectedId }, (link, option) =>
                new SubmitAnonymousTemplateAnswerCommandItem { AnonymousTemplateQuestionId = link.Id, SelectedQuestionOptionId = option }).ToArray()
        };
        var operatorRequest = new SubmitOperatorTemplateResponseCommand
        {
            TemplateId = seed.Authorized.Id,
            Answers = authorizedLinks.Zip(new[] { three.Id, five.Id, selectedId }, (link, option) =>
                new SubmitOperatorTemplateAnswerCommandItem { QuestionId = link.QuestionId, SelectedQuestionOptionId = option }).ToArray()
        };
        var anonAnswers = anonRequest.Answers.ToArray();
        var operatorAnswers = operatorRequest.Answers.ToArray();
        foreach (var (option, anonCode, operatorCode) in new[]
        {
            (Guid.Empty, "DropdownAnswerInvalid", "DropdownInvalidShape"),
            (five.Id, "SelectedOptionNotBelongToQuestion", "DropdownOptionInvalid"),
            (Guid.NewGuid(), "SelectedOptionNotFound", "DropdownOptionInvalid"),
            (inactiveId, "SelectedOptionInactive", "DropdownOptionInvalid")
        })
        {
            var invalidAnon = anonAnswers[2] with { SelectedQuestionOptionId = option };
            var invalidOperator = operatorAnswers[2] with { SelectedQuestionOptionId = option };
            await RejectAsync([anonAnswers[0], anonAnswers[1], invalidAnon],
                [operatorAnswers[0], operatorAnswers[1], invalidOperator], anonCode, operatorCode);
        }
        await RejectAsync([anonAnswers[0], anonAnswers[1], anonAnswers[2] with { TextAnswer = "Mixed" }],
            [operatorAnswers[0], operatorAnswers[1], operatorAnswers[2] with { StarRatingValue = 5 }],
            "DropdownAnswerInvalid", "DropdownInvalidShape");
        await RejectAsync(anonAnswers[..2], operatorAnswers[..2], "VisibleQuestionMissingAnswer", "VisibleQuestionsRequired");
        await RejectAsync([anonAnswers[0] with { SelectedQuestionOptionId = hideChild.Id }, anonAnswers[1], anonAnswers[2]],
            [operatorAnswers[0] with { SelectedQuestionOptionId = hideChild.Id }, operatorAnswers[1], operatorAnswers[2]],
            "HiddenQuestionAnswerNotAllowed", "HiddenQuestionAnswerNotAllowed");

        var anon = await anonymous.Handle(anonRequest, default);
        var normal = await authorized.Handle(operatorRequest, default);
        Assert.True(anon.IsSuccess, string.Join(", ", anon.Errors));
        Assert.True(normal.IsSuccess, string.Join(", ", normal.Errors));
        Assert.Equal((8, 10, 80m), (anon.Value.ActualScore, anon.Value.MaxScore, anon.Value.ScorePercentage));
        Assert.Equal((8, 10, 80m), (normal.Value.ActualScore, normal.Value.MaxScore, normal.Value.ScorePercentage));

        var anonId = anon.Value.AnonymousSurveyResponseId;
        var normalId = normal.Value.SurveyResponseId;
        var savedAnon = await db.Set<AnonymousSurveyAnswer>().SingleAsync(x => x.AnonymousSurveyResponseId == anonId && x.QuestionId == dropdownId);
        var savedNormal = await db.Set<SurveyAnswer>().SingleAsync(x => x.SurveyResponseId == normalId && x.QuestionId == dropdownId);
        Assert.Equal((QuestionType.Dropdown, (Guid?)selectedId), (savedAnon.QuestionType, savedAnon.SelectedQuestionOptionId));
        Assert.Equal((QuestionType.Dropdown, (Guid?)selectedId), (savedNormal.QuestionType, savedNormal.SelectedQuestionOptionId));

        var update = new UpdateQuestionCommandHandler(store.Read<Question>(), store.Read<QuestionGroup>(), CreateQuestionUpdater(store), scope, user);
        var updated = await update.Handle(new()
        {
            QuestionId = dropdownId, GroupId = groupId, TextEn = "Choose a specialty", Type = QuestionType.Dropdown,
            Options = [new() { OptionId = selectedId, TextEn = "Renamed", TextAr = "تعديل", Order = 1 }]
        }, default);
        Assert.True(updated.IsSuccess, string.Join(", ", updated.Errors));
        db.ChangeTracker.Clear();
        foreach (var snapshot in await db.Set<ResponseSnapshot>().ToArrayAsync())
        {
            var document = snapshot.Read();
            var saved = Assert.Single(document.Questions, x => x.QuestionId == dropdownId);
            Assert.Equal((QuestionType.Dropdown, (Guid?)selectedId, "Cardiology", "القلب"),
                (saved.Type, saved.SelectedQuestionOptionId, saved.SelectedOptionTextEn, saved.SelectedOptionTextAr));
            Assert.Null(saved.SelectedOptionValue);
            foreach (var mode in Enum.GetValues<ScoreCalculationMode>())
            {
                var tokens = SnapshotScoring.Tokens(document, mode);
                Assert.Equal(2, tokens.Count);
                Assert.Equal(8m, tokens.Sum(x => x.ScoreValue));
                Assert.DoesNotContain(tokens, x => x.QuestionId == dropdownId);
            }
        }

        var normalDetails = new AuthorizedResponseDetailsReader(store.Read<SurveyResponse>(), store.Read<SurveyAnswer>(),
            store.Read<SurveyResponseCustomInputValue>(), store.Read<QuestionOption>());
        var readNormal = await normalDetails.ReadAsync(normalId, false, default);
        var normalChoice = Assert.Single(readNormal.Value.Answers, x => x.QuestionId == dropdownId);
        Assert.Equal(("Cardiology", "القلب", (Guid?)selectedId, (int?)null),
            (normalChoice.DisplayValue, normalChoice.SelectedOptionTextAr, normalChoice.SelectedQuestionOptionId, normalChoice.SelectedOptionValue));
        var anonDetails = new GetAnonymousTemplateResponseDetailsQueryHandler(store.Read<AnonymousTemplate>(), store.Read<AnonymousSurveyResponse>(),
            store.Read<AnonymousSurveyResponseCustomInputValue>(), store.Read<AnonymousSurveyAnswer>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<QuestionOption>(), store.Read<SuperAdmin>(), scope, user);
        var readAnon = await anonDetails.Handle(new() { AnonymousTemplateId = seed.Anonymous.Id, ResponseId = anonId }, default);
        var anonChoice = Assert.Single(readAnon.Value.Answers, x => x.QuestionId == dropdownId);
        Assert.Equal(("Cardiology", "القلب", (Guid?)selectedId, (int?)null),
            (anonChoice.SelectedOptionTextEn, anonChoice.SelectedOptionTextAr, anonChoice.SelectedQuestionOptionId, anonChoice.SelectedOptionValue));

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
            GeneratedByApplicationUserId = actor, GeneratedByName = "Admin", IncludeResponseDetails = true,
            FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), ToDate = DateOnly.FromDateTime(DateTime.UtcNow)
        }, default);
        Assert.True(report.IsSuccess, string.Join(", ", report.Errors));
        var reportChoice = Assert.Single(Assert.Single(report.Value.Responses).Answers, x => x.QuestionId == dropdownId);
        Assert.Equal(("Cardiology", "القلب", false, false),
            (reportChoice.DisplayValue, reportChoice.SelectedOptionTextAr, reportChoice.IsScorable, reportChoice.IncludedInScore));
        Assert.Null(reportChoice.ScoreValue);
        Assert.Null(reportChoice.SelectedOptionValue);
        Assert.DoesNotContain(report.Value.WorstQuestions.Concat(report.Value.BestQuestions), x => x.TemplateQuestionId == authorizedLinks[2].Id);
        Assert.Equal(2, report.Value.QuestionGroupAnalytics.Sum(x => x.ScorableQuestionsCount));

        async Task RejectAsync(SubmitAnonymousTemplateAnswerCommandItem[] anonInput, SubmitOperatorTemplateAnswerCommandItem[] operatorInput,
            string anonCode, string operatorCode)
        {
            var invalidAnon = await anonymous.Handle(anonRequest with { Answers = anonInput }, default);
            var invalidNormal = await authorized.Handle(operatorRequest with { Answers = operatorInput }, default);
            Assert.Equal("AnonTemplates.Submit." + anonCode, Assert.Single(invalidAnon.Errors).Code);
            Assert.Equal("SurveyResponses.Submit." + operatorCode, Assert.Single(invalidNormal.Errors).Code);
        }
    }

    private static SubmitAnonymousTemplateResponseCommandHandler CreateDropdownAnonymousHandler(OptionalQuestionStore store) => new(
        store.Read<AnonymousTemplate>(), store.Read<AnonymousTemplateCustomInput>(), store.Read<AnonymousTemplateQuestion>(),
        store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>(), store.Write<AnonymousSurveyResponse>(),
        new UnusedSubmissionMedia(), store.Unit);
}
