using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed partial class SurveyHistorySqlTests
{
    [SqlHistoryFact]
    public async Task Global_question_type_change_protects_active_conditions_across_all_template_kinds()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedTemplatesAsync(db);
        var user = new TestCurrentUser(seed.User.Id);
        var group = QuestionGroup.CreateGlobal("Original group", null, seed.User.Id);
        var nextGroup = QuestionGroup.CreateGlobal("Updated group", null, seed.User.Id);
        var question = Question.CreateGlobal(group.Id, "Original", null, QuestionType.SingleChoice, seed.User.Id);
        var child = Question.CreateGlobal(group.Id, "Other", null, QuestionType.StarRating, seed.User.Id);
        var option = QuestionOption.Create(question.Id, "First", null, 1, 5, seed.User.Id);
        var secondOption = QuestionOption.Create(question.Id, "Second", null, 2, 1, seed.User.Id);
        var source = AnonymousTemplate.CreateGlobalTemplate("Global source", null, null, DateTime.UtcNow.AddDays(-1), null, seed.User.Id);
        var managed = AnonymousTemplate.CreateBranchTemplate(seed.Branch.Id, "Managed copy", null, null,
            source.ActiveFrom, null, seed.User.Id, sourceGlobalAnonymousTemplateId: source.Id);
        seed.Authorized.Deactivate();
        seed.Anonymous.Deactivate();
        managed.Deactivate();
        db.AddRange(group, nextGroup, question, child, option, secondOption, source, managed,
            SuperAdmin.Create(seed.User.Id, seed.User.Id));
        await db.SaveChangesAsync();

        var authorizedParent = TemplateQuestion.Create(seed.Authorized.Id, question.Id, 1, seed.User.Id);
        var authorizedChild = TemplateQuestion.Create(seed.Authorized.Id, child.Id, 2, seed.User.Id);
        db.AddRange(authorizedParent, authorizedChild);

        var anonymousLinks = new[] { source, seed.Anonymous, managed }.Select(template => (
            Parent: AnonymousTemplateQuestion.Create(template.Id, question.Id, 1, seed.User.Id),
            Child: AnonymousTemplateQuestion.Create(template.Id, child.Id, 2, seed.User.Id))).ToArray();
        foreach (var links in anonymousLinks)
        {
            db.AddRange(links.Parent, links.Child);
        }
        var childOnly = TemplateQuestionCondition.CreateForStarRating(seed.Authorized.Id, authorizedChild.Id, authorizedParent.Id, 4, 1, seed.User.Id);
        db.Add(childOnly);
        await db.SaveChangesAsync();

        var authorizedCondition = TemplateQuestionCondition.CreateForSingleChoice(
            seed.Authorized.Id, authorizedParent.Id, authorizedChild.Id, option.Id, 1, seed.User.Id);
        db.Add(authorizedCondition);
        await db.SaveChangesAsync();

        var handler = new UpdateGlobalQuestionCommandHandler(store.Read<Question>(), store.Read<QuestionGroup>(),
            CreateQuestionUpdater(store), store.Read<SuperAdmin>(), user);
        var changeType = new UpdateGlobalQuestionCommand
        {
            QuestionId = question.Id, GroupId = nextGroup.Id, TextEn = "Updated", Type = QuestionType.StarRating
        };
        var omitProtectedOption = changeType with
        {
            Type = QuestionType.SingleChoice,
            Options = [
                new() { OptionId = secondOption.Id, TextEn = "Second", Order = 2, Value = 1 },
                new() { TextEn = "Replacement", Order = 1, Value = 4 }]
        };
        db.ChangeTracker.Clear();
        var sameType = await handler.Handle(omitProtectedOption with
        {
            GroupId = group.Id, TextEn = question.TextEn,
            Options = [
                new() { OptionId = option.Id, TextEn = option.TextEn, Order = 1, Value = 5 },
                new() { OptionId = secondOption.Id, TextEn = secondOption.TextEn, Order = 2, Value = 1 }]
        }, default);
        Assert.True(sameType.IsSuccess, string.Join(", ", sameType.Errors));
        await AssertProtectedAsync();
        var protectedAuthorized = await db.Set<TemplateQuestionCondition>().SingleAsync(x => x.Id == authorizedCondition.Id);
        Assert.True(protectedAuthorized.IsActive);
        protectedAuthorized.Deactivate();
        await db.SaveChangesAsync();

        foreach (var (parent, dependent) in anonymousLinks)
        {
            var condition = AnonymousTemplateQuestionCondition.CreateForSingleChoice(
                parent.AnonymousTemplateId, parent.Id, dependent.Id, option.Id, 1, seed.User.Id);
            db.Add(condition);
            await db.SaveChangesAsync();
            await AssertProtectedAsync();
            var protectedAnonymous = await db.Set<AnonymousTemplateQuestionCondition>().SingleAsync(x => x.Id == condition.Id);
            Assert.True(protectedAnonymous.IsActive);
            protectedAnonymous.Deactivate();
            await db.SaveChangesAsync();
        }

        db.ChangeTracker.Clear();
        var changed = await handler.Handle(changeType, default);
        Assert.True(changed.IsSuccess, string.Join(", ", changed.Errors));
        Assert.Equal(question.Id, changed.Value.QuestionId);
        Assert.Equal(QuestionType.StarRating, changed.Value.Type);
        Assert.Empty(changed.Value.Options);
        Assert.Equal(authorizedParent.Id, await db.Set<TemplateQuestion>().Where(x => x.QuestionId == question.Id).Select(x => x.Id).SingleAsync());
        var storedLinks = await db.Set<AnonymousTemplateQuestion>().Where(x => x.QuestionId == question.Id).Select(x => x.Id).ToArrayAsync();
        Assert.Equal(anonymousLinks.Select(x => x.Parent.Id).Order(), storedLinks.Order());
        Assert.Equal(source.Id, await db.Set<AnonymousTemplate>().Where(x => x.Id == managed.Id).Select(x => x.SourceGlobalAnonymousTemplateId).SingleAsync());
        Assert.All(await db.Set<QuestionOption>().Where(x => x.QuestionId == question.Id).ToArrayAsync(), x => Assert.False(x.IsActive));
        Assert.True((await db.Set<TemplateQuestionCondition>().AsNoTracking().SingleAsync(x => x.Id == childOnly.Id)).IsActive);

        // Older updates could deactivate a choice even while an anonymous condition still referenced it.
        var managedLinks = anonymousLinks[^1];
        db.Add(AnonymousTemplateQuestionCondition.CreateForSingleChoice(
            managed.Id, managedLinks.Parent.Id, managedLinks.Child.Id, secondOption.Id, 1, seed.User.Id));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var restoreChoices = omitProtectedOption with
        {
            Options = [new() { OptionId = option.Id, TextEn = option.TextEn, Order = 1, Value = 5 }]
        };
        var missingChoice = await handler.Handle(restoreChoices, default);
        Assert.Equal("GlobalQuestions.Update.OptionUsedInCondition", Assert.Single(missingChoice.Errors).Code);
        var restored = await handler.Handle(restoreChoices with
        {
            Options = [new() { OptionId = secondOption.Id, TextEn = secondOption.TextEn, Order = 1, Value = 1 }]
        }, default);
        Assert.True(restored.IsSuccess, string.Join(", ", restored.Errors));
        Assert.Equal(secondOption.Id, Assert.Single(restored.Value.Options).OptionId);
        Assert.False(await db.Set<QuestionOption>().Where(x => x.Id == option.Id).Select(x => x.IsActive).SingleAsync());

        async Task AssertProtectedAsync()
        {
            db.ChangeTracker.Clear();
            var typeResult = await handler.Handle(changeType, default);
            Assert.Equal("GlobalQuestions.Update.TypeChangeIncompatibleWithConditions", Assert.Single(typeResult.Errors).Code);
            var textResult = await handler.Handle(changeType with { Type = QuestionType.FreeText }, default);
            Assert.Equal("GlobalQuestions.Update.TypeChangeIncompatibleWithConditions", Assert.Single(textResult.Errors).Code);
            var optionResult = await handler.Handle(omitProtectedOption, default);
            Assert.Equal("GlobalQuestions.Update.OptionUsedInCondition", Assert.Single(optionResult.Errors).Code);
            var saved = await db.Set<Question>().AsNoTracking().SingleAsync(x => x.Id == question.Id);
            Assert.Equal((QuestionType.SingleChoice, "Original", group.Id), (saved.Type, saved.TextEn, saved.GroupId));
            var options = await db.Set<QuestionOption>().AsNoTracking().Where(x => x.QuestionId == question.Id).OrderBy(x => x.Order).ToArrayAsync();
            Assert.Equal(new[] { option.Id, secondOption.Id }, options.Select(x => x.Id));
            Assert.All(options, x => Assert.True(x.IsActive));
        }
    }

    [SqlHistoryFact]
    public async Task Branch_question_type_change_preserves_authorized_and_anonymous_submission_history()
    {
        await using var store = new OptionalQuestionStore();
        var db = store.Db;
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedTemplatesAsync(db);
        var actor = seed.User.Id;
        var user = new TestCurrentUser(actor);
        var scope = new AssignmentScope(seed.Branch.Id, actor);
        var question = seed.Questions[0];
        question.Update(question.GroupId, "Original question", null, QuestionType.SingleChoice);
        var selected = QuestionOption.Create(question.Id, "Excellent", null, 1, 5, actor);
        var retired = QuestionOption.Create(question.Id, "Other", null, 2, 1, actor);
        var authorizedLink = TemplateQuestion.Create(seed.Authorized.Id, question.Id, 1, actor);
        var anonymousLink = AnonymousTemplateQuestion.Create(seed.Anonymous.Id, question.Id, 1, actor);
        db.AddRange(selected, retired, authorizedLink, anonymousLink, OperatorTemplate.Create(seed.Operator.Id, seed.Authorized.Id, actor));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var branchUpdate = new UpdateQuestionCommandHandler(store.Read<Question>(), store.Read<QuestionGroup>(), CreateQuestionUpdater(store), scope, user);
        var update = new UpdateQuestionCommand
        {
            QuestionId = question.Id, GroupId = question.GroupId, TextEn = "Rating question", Type = QuestionType.StarRating
        };
        var authorizedSubmit = new SubmitOperatorTemplateResponseCommandHandler(store.Read<Operator>(),
            store.Read<OperatorTemplate>(), store.Read<TemplateQuestion>(), store.Read<TemplateCustomInput>(),
            store.Read<QuestionOption>(), store.Read<TemplateQuestionCondition>(), store.Write<SurveyResponse>(),
            new UnusedSubmissionMedia(), user, store.Unit, new FlagPermissions(store.Read<UserRole>()));
        var anonymousSubmit = new SubmitAnonymousTemplateResponseCommandHandler(store.Read<AnonymousTemplate>(),
            store.Read<AnonymousTemplateCustomInput>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<AnonymousTemplateQuestionCondition>(), store.Read<QuestionOption>(), store.Write<AnonymousSurveyResponse>(),
            new UnusedSubmissionMedia(), store.Unit);
        var authorizedDetails = new AuthorizedResponseDetailsReader(store.Read<SurveyResponse>(), store.Read<SurveyAnswer>(),
            store.Read<SurveyResponseCustomInputValue>(), store.Read<QuestionOption>());
        var anonymousDetails = new GetAnonymousTemplateResponseDetailsQueryHandler(store.Read<AnonymousTemplate>(), store.Read<AnonymousSurveyResponse>(),
            store.Read<AnonymousSurveyResponseCustomInputValue>(), store.Read<AnonymousSurveyAnswer>(), store.Read<AnonymousTemplateQuestion>(),
            store.Read<QuestionOption>(), store.Read<SuperAdmin>(), scope, user);

        var originalAuthorized = (await authorizedSubmit.Handle(AuthorizedAnswer(selected.Id), default)).Value;
        var originalAnonymous = (await anonymousSubmit.Handle(AnonymousAnswer(selected.Id), default)).Value;
        var legacyAnonymous = (await anonymousSubmit.Handle(AnonymousAnswer(selected.Id), default)).Value;
        var originalIds = new[] { originalAuthorized.SurveyResponseId, originalAnonymous.AnonymousSurveyResponseId };
        var originalPayloads = await db.Set<ResponseSnapshot>().AsNoTracking().Where(x => originalIds.Contains(x.ResponseId))
            .ToDictionaryAsync(x => x.ResponseId, x => x.Payload);
        Assert.Equal(2, originalPayloads.Count);
        // Simulate a legacy response stored before snapshot capture was available.
        await db.Set<ResponseSnapshot>().Where(x => x.ResponseId == legacyAnonymous.AnonymousSurveyResponseId).ExecuteDeleteAsync();
        db.ChangeTracker.Clear();

        var rated = await branchUpdate.Handle(update, default);
        Assert.True(rated.IsSuccess, string.Join(", ", rated.Errors));
        Assert.Equal(question.Id, rated.Value.QuestionId);
        Assert.Equal(QuestionType.StarRating, rated.Value.Type);
        Assert.Empty(rated.Value.Options);
        Assert.All(await db.Set<QuestionOption>().Where(x => x.QuestionId == question.Id).ToArrayAsync(), x => Assert.False(x.IsActive));
        var legacy = (await ReadAnonymousAsync(legacyAnonymous.AnonymousSurveyResponseId)).Value;
        Assert.Null(legacy.HistoricalSurvey);
        Assert.Equal(QuestionType.SingleChoice, Assert.Single(legacy.Answers).QuestionType);
        Assert.Equal(100m, legacy.ScorePercentage);

        var oldAuthorizedShape = await authorizedSubmit.Handle(AuthorizedAnswer(selected.Id), default);
        var oldAnonymousShape = await anonymousSubmit.Handle(AnonymousAnswer(selected.Id), default);
        Assert.Contains(oldAuthorizedShape.Errors, x => x.Code.EndsWith("StarRatingValueInvalid", StringComparison.Ordinal));
        Assert.Contains(oldAnonymousShape.Errors, x => x.Code.EndsWith("StarRatingAnswerInvalid", StringComparison.Ordinal));
        var newAuthorized = (await authorizedSubmit.Handle(AuthorizedAnswer(stars: 4), default)).Value;
        var newAnonymous = (await anonymousSubmit.Handle(AnonymousAnswer(stars: 4), default)).Value;

        db.ChangeTracker.Clear();
        var restored = await branchUpdate.Handle(update with
        {
            TextEn = "New choices",
            Type = QuestionType.SingleChoice,
            Options = [
                new() { TextEn = "New option", Order = 1, Value = 2 },
                new() { OptionId = selected.Id, TextEn = "New label", Order = 2, Value = 3 }]
        }, default);
        Assert.True(restored.IsSuccess, string.Join(", ", restored.Errors));
        Assert.Equal(new[] { 1, 2 }, restored.Value.Options.Select(x => x.Order));
        Assert.Equal(selected.Id, restored.Value.Options.Last().OptionId);
        Assert.All(restored.Value.Options, x => Assert.True(x.IsActive));
        var savedOptions = await db.Set<QuestionOption>().AsNoTracking().Where(x => x.QuestionId == question.Id).ToDictionaryAsync(x => x.Id);
        Assert.Equal(3, savedOptions.Count);
        Assert.False(savedOptions[retired.Id].IsActive);
        Assert.True(savedOptions[selected.Id].IsActive);
        Assert.Equal("New label", savedOptions[selected.Id].TextEn);
        Assert.Equal(3, savedOptions[selected.Id].Value);
        Assert.Equal(authorizedLink.Id, await db.Set<TemplateQuestion>().Select(x => x.Id).SingleAsync());
        Assert.Equal(anonymousLink.Id, await db.Set<AnonymousTemplateQuestion>().Select(x => x.Id).SingleAsync());

        var authorizedOldDetails = (await authorizedDetails.ReadAsync(originalAuthorized.SurveyResponseId, false, default)).Value;
        var anonymousOldDetails = (await ReadAnonymousAsync(originalAnonymous.AnonymousSurveyResponseId)).Value;
        var authorizedOldAnswer = Assert.Single(authorizedOldDetails.Answers);
        var anonymousOldAnswer = Assert.Single(anonymousOldDetails.Answers);
        Assert.Equal((QuestionType.SingleChoice, "Original question", (Guid?)selected.Id, "Excellent", (int?)5),
            (authorizedOldAnswer.QuestionType, authorizedOldAnswer.QuestionTextEn, authorizedOldAnswer.SelectedQuestionOptionId,
                authorizedOldAnswer.SelectedOptionTextEn, authorizedOldAnswer.SelectedOptionValue));
        Assert.Equal((QuestionType.SingleChoice, "Original question", (Guid?)selected.Id, "Excellent", (int?)5),
            (anonymousOldAnswer.QuestionType, anonymousOldAnswer.QuestionTextEn, anonymousOldAnswer.SelectedQuestionOptionId,
                anonymousOldAnswer.SelectedOptionTextEn, anonymousOldAnswer.SelectedOptionValue));
        Assert.Equal((5, 5, 100m, originalAuthorized.SubmittedOnUtc),
            (authorizedOldDetails.Score.ActualScore, authorizedOldDetails.Score.MaxScore, authorizedOldDetails.Score.ScorePercentage, authorizedOldDetails.SubmittedOnUtc));
        Assert.Equal((5, 5, 100m, originalAnonymous.SubmittedOnUtc),
            (anonymousOldDetails.ActualScore, anonymousOldDetails.MaxScore, anonymousOldDetails.ScorePercentage, anonymousOldDetails.SubmittedOnUtc));
        var authorizedNewDetails = (await authorizedDetails.ReadAsync(newAuthorized.SurveyResponseId, false, default)).Value;
        var anonymousNewDetails = (await ReadAnonymousAsync(newAnonymous.AnonymousSurveyResponseId)).Value;
        Assert.Equal((4, 5, 80m), (authorizedNewDetails.Score.ActualScore, authorizedNewDetails.Score.MaxScore, authorizedNewDetails.Score.ScorePercentage));
        Assert.Equal((4, 5, 80m), (anonymousNewDetails.ActualScore, anonymousNewDetails.MaxScore, anonymousNewDetails.ScorePercentage));
        foreach (var snapshot in await db.Set<ResponseSnapshot>().AsNoTracking().ToArrayAsync())
        {
            var historical = snapshot.Read();
            Assert.True(historical.IsHistoricallyVerified);
            var captured = Assert.Single(historical.Questions);
            Assert.Equal(question.Id, captured.QuestionId);
            var isOriginal = originalPayloads.TryGetValue(snapshot.ResponseId, out var payload);
            if (isOriginal)
            {
                Assert.Equal(payload, snapshot.Payload);
                Assert.Equal(QuestionType.SingleChoice, captured.Type);
            }
            else
            {
                Assert.Equal(QuestionType.StarRating, captured.Type);
                Assert.Equal(4, captured.StarRatingValue);
            }

            foreach (var mode in Enum.GetValues<ScoreCalculationMode>())
            {
                Assert.Equal(isOriginal ? 5 : 4, Assert.Single(SnapshotScoring.Tokens(historical, mode)).ScoreValue);
            }
        }
        Assert.Equal(QuestionType.StarRating, Assert.Single(authorizedNewDetails.Answers).QuestionType);
        Assert.Equal(QuestionType.StarRating, Assert.Single(anonymousNewDetails.Answers).QuestionType);

        SubmitOperatorTemplateResponseCommand AuthorizedAnswer(Guid? optionId = null, int? stars = null) => new()
        {
            TemplateId = seed.Authorized.Id,
            Answers = [new() { QuestionId = question.Id, SelectedQuestionOptionId = optionId, StarRatingValue = stars }]
        };

        SubmitAnonymousTemplateResponseCommand AnonymousAnswer(Guid? optionId = null, int? stars = null) => new()
        {
            AnonymousTemplateId = seed.Anonymous.Id,
            Answers = [new() { AnonymousTemplateQuestionId = anonymousLink.Id, SelectedQuestionOptionId = optionId, StarRatingValue = stars }]
        };

        Task<BuildingBlock.Domain.Results.Result<GetAnonymousTemplateResponseDetailsResponse>> ReadAnonymousAsync(Guid responseId) =>
            anonymousDetails.Handle(new GetAnonymousTemplateResponseDetailsQuery
            {
                AnonymousTemplateId = seed.Anonymous.Id, ResponseId = responseId
            }, default);
    }

    private static QuestionDefinitionUpdater CreateQuestionUpdater(OptionalQuestionStore store) => new(
        store.Write<Question>(), store.Read<QuestionOption>(), store.Write<QuestionOption>(),
        store.Read<TemplateQuestionCondition>(), store.Read<AnonymousTemplateQuestionCondition>(), store.Unit);
}
