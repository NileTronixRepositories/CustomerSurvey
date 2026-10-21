using System.Reflection;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Branches.EvaluationSettings;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Xunit;
using DomainUserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Tests.Reports;

public sealed class HistoricalScoringAndSettingsTests
{
    [Theory]
    [InlineData(ScoreCalculationMode.RootQuestions, 100)]
    [InlineData(ScoreCalculationMode.LowestConditionLevel, 20)]
    [InlineData(ScoreCalculationMode.AllAnsweredQuestions, 60)]
    public void Snapshot_scoring_covers_all_answered_branches_and_nonnumeric_fallback(ScoreCalculationMode mode, decimal expected)
    {
        var root = Q(5, true);
        var child = Q(1, false);
        var otherBranch = Q(3, false);
        var text = Q(null, false) with { Type = QuestionType.FreeText, TextAnswer = "feedback" };
        var snapshot = Document(root, child, otherBranch, text) with
        {
            Conditions = new[]
            {
                new SnapshotCondition(root.TemplateQuestionId, child.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, null, 5, 1),
                new SnapshotCondition(root.TemplateQuestionId, otherBranch.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, null, 5, 2),
                new SnapshotCondition(child.TemplateQuestionId, text.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, null, 1, 1)
            }
        };
        var score = Assert.Single(new SurveyReportScoringService().CalculateResponseScores(
            new[] { Flat(snapshot) }, Array.Empty<AnswerFlatDto>(), Array.Empty<TemplateQuestionFlatDto>(),
            Array.Empty<ConditionFlatDto>(), Array.Empty<QuestionOptionFlatDto>(), mode));
        Assert.Equal(expected, score.ScorePercentage);
        Assert.Equal(mode == ScoreCalculationMode.AllAnsweredQuestions ? 3 : 1, score.ScoredItemsCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Snapshot_roundtrip_preserves_options_labels_media_and_original_scores(int kind)
    {
        var selected = Guid.NewGuid();
        var unselected = Guid.NewGuid();
        var question = Q(null, true) with
        {
            Type = QuestionType.SingleChoice, TextEn = "Original", TextAr = "قديم",
            SelectedQuestionOptionId = selected, SelectedOptionValue = 3, SelectedOptionTextEn = "Original option",
            Options = new[] { new SnapshotOption(selected, "Original option", null, 3, 1, true), new SnapshotOption(unselected, "Other", null, 1, 2, true) },
            VoiceFileName = "stored.mp3", ImageFileName = "stored.png"
        };
        var document = Document(question) with { TemplateKind = kind, ActualScore = 3, MaxScore = 5, ScorePercentage = 60m };
        var snapshot = ResponseSnapshot.Create(document);
        question = question with { TextEn = "Changed", SelectedOptionValue = 5 };
        var saved = Assert.Single(snapshot.Read().Questions);
        Assert.Equal("Original", saved.TextEn);
        Assert.Equal(3, saved.SelectedOptionValue);
        Assert.Equal(2, saved.Options.Count);
        Assert.Equal("stored.mp3", saved.VoiceFileName);
        Assert.Equal("stored.png", saved.ImageFileName);
        Assert.Equal(60m, snapshot.RootScorePercentage);
        Assert.Equal(60m, snapshot.Read().ScorePercentage);
        Assert.Equal(kind == 1 ? document.ResponseId : null, snapshot.SurveyResponseId);
        Assert.Equal(kind == 2 ? document.ResponseId : null, snapshot.AnonymousSurveyResponseId);
    }

    [Fact]
    public void Aggregates_weight_each_scored_response_equally()
    {
        var first = Document(Q(5, true));
        var second = Document(Q(1, true), Q(1, true), Q(1, true));
        var scores = new SurveyReportScoringService().CalculateResponseScores(new[] { Flat(first), Flat(second) },
            Array.Empty<AnswerFlatDto>(), Array.Empty<TemplateQuestionFlatDto>(), Array.Empty<ConditionFlatDto>(),
            Array.Empty<QuestionOptionFlatDto>(), ScoreCalculationMode.AllAnsweredQuestions);
        Assert.Equal(60m, scores.Average(x => x.ScorePercentage));
    }

    [Theory]
    [InlineData(SnapshotProvenance.LegacyIncomplete)]
    [InlineData(SnapshotProvenance.LegacyReconstructed)]
    public void Incomplete_legacy_modes_expose_stored_baseline_and_uncertainty(SnapshotProvenance provenance)
    {
        var doc = Document(Q(1, true)) with { Provenance = provenance };
        var response = Flat(doc) with { OriginalMaxScore = 3, OriginalScorePercentage = 100m };
        var service = new SurveyReportScoringService();
        var scores = service.CalculateResponseScores(new[] { response }, Array.Empty<AnswerFlatDto>(),
            Array.Empty<TemplateQuestionFlatDto>(), Array.Empty<ConditionFlatDto>(), Array.Empty<QuestionOptionFlatDto>(), ScoreCalculationMode.AllAnsweredQuestions);
        var score = Assert.Single(scores);
        Assert.Equal(100m, score.ScorePercentage);
        Assert.True(score.HasScore);
        Assert.False(score.RequestedModeAvailable);
        Assert.Equal("StoredRootBaseline", score.ScoreDerivation);
        Assert.Empty(service.CalculateQuestionScoreTokens(new[] { response }, Array.Empty<AnswerFlatDto>(),
            Array.Empty<TemplateQuestionFlatDto>(), Array.Empty<ConditionFlatDto>(), Array.Empty<QuestionOptionFlatDto>(), ScoreCalculationMode.AllAnsweredQuestions));
    }

    [Theory]
    [InlineData(3, 60)]
    [InlineData(5, 100)]
    public void New_anonymous_single_choice_always_has_maximum_five(int optionValue, decimal percentage)
    {
        var questionId = Guid.NewGuid();
        var templateQuestionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var method = typeof(SubmitAnonymousTemplateResponseCommandHandler).GetMethod("CalculateScore", BindingFlags.Static | BindingFlags.NonPublic)!;
        var score = method.Invoke(null, new object[]
        {
            new[] { new AnonymousTemplateQuestionForSubmitDto { AnonymousTemplateQuestionId = templateQuestionId, QuestionId = questionId, QuestionType = QuestionType.SingleChoice } },
            new[] { templateQuestionId },
            new[] { new SubmitAnonymousTemplateAnswerCommandItem { AnonymousTemplateQuestionId = templateQuestionId, SelectedQuestionOptionId = optionId } },
            new[] { new QuestionOptionForSubmitDto { OptionId = optionId, QuestionId = questionId, Value = optionValue, IsActive = true } }
        })!;
        Assert.Equal(5, score.GetType().GetProperty("MaxScore")!.GetValue(score));
        Assert.Equal(percentage, score.GetType().GetProperty("ScorePercentage")?.GetValue(score) ?? score.GetType().GetProperty("Percentage")!.GetValue(score));
    }

    [Theory]
    [InlineData(-1, 80, 50, 75)]
    [InlineData(80, 80, 50, 75)]
    [InlineData(60, 101, 50, 75)]
    [InlineData(60, 80, 75, 75)]
    [InlineData(60, 80, -1, 75)]
    public void Invalid_thresholds_are_rejected(decimal neutral, decimal satisfied, decimal medium, decimal healthy)
        => Assert.False(BranchEvaluationSettings.IsValid(neutral, satisfied, medium, healthy));

    [Fact]
    public void Current_criteria_are_independent_and_unscored_never_becomes_high_risk()
    {
        var criteria = new EvaluationCriteria { NeutralMinimum = 50m, SatisfiedMinimum = 90m, MediumRiskMinimum = 10m, HealthyMinimum = 20m };
        Assert.Equal(SatisfactionCategory.Neutral, SatisfactionCategoryRule.Classify(5, 85m, criteria));
        Assert.Equal("Healthy", criteria.Risk(85m));
        Assert.Equal("NoData", criteria.Risk(null));
        Assert.Null(SatisfactionCategoryRule.Classify(0, 0m, criteria));
    }

    [Theory]
    [InlineData(DomainUserType.BranchAdmin, false, true)]
    [InlineData(DomainUserType.BranchAdmin, true, false)]
    [InlineData(DomainUserType.BranchUser, false, false)]
    [InlineData(DomainUserType.BranchArea, false, false)]
    public async Task Settings_update_enforces_actor_and_branch_scope(DomainUserType actorType, bool otherBranch, bool allowed)
    {
        var branch = Guid.NewGuid();
        var user = new SettingsUser(actorType);
        var write = new InMemoryWriteRepository<BranchEvaluationSettings>();
        var uow = new TestUnitOfWork();
        var handler = new BranchEvaluationSettingsHandlers(new Scope(branch, actorType, user.UserId!.Value), user,
            new InMemoryReadRepository<BranchEvaluationSettings>(), write, uow);
        var result = await handler.Handle(new UpdateBranchEvaluationSettingsCommand
        {
            BranchId = otherBranch ? Guid.NewGuid() : branch, NeutralMinimum = 55m, SatisfiedMinimum = 90m,
            MediumRiskMinimum = 25m, HealthyMinimum = 70m
        }, CancellationToken.None);
        Assert.Equal(allowed, result.IsSuccess);
        Assert.Equal(allowed ? 1 : 0, uow.SaveChangesCount);
        Assert.Equal(allowed ? 1 : 0, write.AddCount);
    }

    [Theory]
    [InlineData(ScoreCalculationMode.RootQuestions, 100)]
    [InlineData(ScoreCalculationMode.LowestConditionLevel, 20)]
    [InlineData(ScoreCalculationMode.AllAnsweredQuestions, 60)]
    public void Pagination_uses_selected_snapshot_score_before_filtering(ScoreCalculationMode mode, decimal expected)
    {
        var response = SurveyResponse.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, 5, 100m);
        var root = Q(5, true); var child = Q(1, false);
        response.CaptureSnapshot(Document(root, child) with { ResponseId = response.Id, TemplateId = response.TemplateId,
            ActualScore = 5, MaxScore = 5, ScorePercentage = 100m,
            Conditions = new[] { new SnapshotCondition(root.TemplateQuestionId, child.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, null, 5, 1) } });
        var spec = new GetBranchSurveyResponsesPaginationSpec(null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1),
            new GetBranchSurveyResponsesPaginationQuery { ScoreCalculationMode = mode, MinScorePercentage = expected, MaxScorePercentage = expected });
        Assert.True(spec.Criteria.Compile()(response));
        Assert.Equal(100m, response.ScorePercentage);
        Assert.Throws<InvalidOperationException>(() => response.CaptureSnapshot(response.Snapshot!.Read()));
    }

    internal static SnapshotQuestion Q(int? value, bool root) => new()
    {
        TemplateQuestionId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), Type = QuestionType.StarRating,
        TextEn = "Historical question", GroupId = Guid.NewGuid(), GroupNameEn = "Group", IsRoot = root,
        HasAnswer = true, StarRatingValue = value
    };

    [Fact]
    public void Question_group_averages_give_equal_weight_to_responses()
    {
        var template = Guid.NewGuid(); var group = Guid.NewGuid();
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        var questions = Enumerable.Range(1, 3).Select(i => new TemplateQuestionFlatDto
        {
            TemplateKind = ReportTemplateKind.Normal, TemplateId = template, TemplateQuestionId = Guid.NewGuid(),
            QuestionId = Guid.NewGuid(), QuestionType = QuestionType.StarRating, QuestionGroupId = group,
            QuestionGroupNameEn = "Historic group", Order = i
        }).ToArray();
        var tokens = questions.Select(q => new QuestionScoreToken { TemplateId = template, TemplateKind = ReportTemplateKind.Normal,
            TemplateQuestionId = q.TemplateQuestionId, QuestionId = q.QuestionId, ResponseId = second, ScoreValue = 1m }).ToList();
        tokens.Add(tokens[0] with { ResponseId = first, ScoreValue = 5m });
        var method = typeof(CustomerSurvey.infrastructure.Reports.BranchTemplatesPdfReportService)
            .GetMethod("BuildQuestionGroupAnalytics", BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (IReadOnlyCollection<BranchTemplatesReportQuestionGroupAnalytics>)method.Invoke(null, new object[]
        {
            new[] { new BranchTemplatesPdfTemplateSummary { TemplateId = template, TemplateKind = ReportTemplateKind.Normal } },
            new[] { new ResponseFlatDto { ResponseId = first, TemplateId = template, TemplateKind = ReportTemplateKind.Normal },
                new ResponseFlatDto { ResponseId = second, TemplateId = template, TemplateKind = ReportTemplateKind.Normal } }, questions, tokens.ToArray()
        })!;
        var row = Assert.Single(result);
        Assert.Equal(60m, row.AverageScorePercentage);
        Assert.Equal(2, row.ScoredResponsesCount);
        Assert.Equal(4, row.ScoredItemsCount);
    }

    [Fact]
    public void Best_questions_apply_cutoff_answer_count_ties_and_limit()
    {
        var handler = typeof(CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.GetBranchDashboardQueryHandler);
        var period = Activator.CreateInstance(handler.GetNestedType("ResolvedDashboardPeriod", BindingFlags.NonPublic)!,
            new object[] { new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), false })!;
        var first = Guid.NewGuid(); var second = Guid.NewGuid(); var low = Guid.NewGuid();
        var answers = new[] { (first, 5m), (second, 5m), (second, 5m), (low, 3m) }.Select(x =>
            new CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.DashboardSurveyAnswerDto
            { QuestionId = x.Item1, TemplateId = Guid.Empty, QuestionTextEn = Guid.NewGuid().ToString(), QuestionType = QuestionType.SingleChoice, ReportScoreValue = x.Item2 }).ToArray();
        var method = handler.GetMethod("BuildLowestRatedQuestions", BindingFlags.NonPublic | BindingFlags.Static)!;
        var result = (IReadOnlyCollection<CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.BranchDashboardQuestionInsightResponse>)method.Invoke(null,
            new object[] { answers, new CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.GetBranchDashboardQuery { TopQuestionsCount = 1 }, period, true })!;
        Assert.Equal(second, Assert.Single(result).QuestionId);
        var empty = (IReadOnlyCollection<CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.BranchDashboardQuestionInsightResponse>)method.Invoke(null,
            new object[] { answers.Where(x => x.QuestionId == low).ToArray(), new CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard.GetBranchDashboardQuery(), period, true })!;
        Assert.Empty(empty);
    }

    [Fact]
    public void Group_percentages_round_after_response_weighting()
    {
        var first = Guid.NewGuid(); var second = Guid.NewGuid();
        var tokens = new[]
        {
            new QuestionScoreToken { ResponseId = first, ScoreValue = 1m },
            new QuestionScoreToken { ResponseId = first, ScoreValue = 2m },
            new QuestionScoreToken { ResponseId = first, ScoreValue = 2m },
            new QuestionScoreToken { ResponseId = second, ScoreValue = 4m }
        };
        Assert.Equal(56.67m, ReportScoreAggregation.ResponseWeightedPercentage(tokens));
    }

    [Fact]
    public void Historical_group_membership_survives_moving_a_question()
    {
        var original = Q(5, true);
        var moved = original with { GroupId = Guid.NewGuid(), GroupNameEn = "New group", StarRatingValue = 1 };
        var first = Document(original) with { SubmittedOnUtc = DateTime.UtcNow.AddDays(-1) };
        var second = Document(moved) with { TemplateId = first.TemplateId };
        var responses = new[] { Flat(first), Flat(second) };
        var definitions = SnapshotScoring.HistoricalQuestions(responses, Array.Empty<TemplateQuestionFlatDto>(), preserveGroupHistory: true);
        Assert.Equal(2, definitions.Count);
        var tokens = SnapshotScoring.Tokens(first, ScoreCalculationMode.RootQuestions).Concat(SnapshotScoring.Tokens(second, ScoreCalculationMode.RootQuestions)).ToArray();
        var method = typeof(CustomerSurvey.infrastructure.Reports.BranchTemplatesPdfReportService).GetMethod("BuildQuestionGroupAnalytics", BindingFlags.Static | BindingFlags.NonPublic)!;
        var groups = (IReadOnlyCollection<BranchTemplatesReportQuestionGroupAnalytics>)method.Invoke(null, new object[]
        {
            new[] { new BranchTemplatesPdfTemplateSummary { TemplateId = first.TemplateId, TemplateKind = ReportTemplateKind.Normal } }, responses, definitions, tokens
        })!;
        Assert.Equal(100m, groups.Single(x => x.QuestionGroupId == original.GroupId).AverageScorePercentage);
        Assert.Equal(20m, groups.Single(x => x.QuestionGroupId == moved.GroupId).AverageScorePercentage);
        Assert.All(groups, x => Assert.Equal(1, x.ScoredResponsesCount));
    }

    [Fact]
    public void Settings_routes_require_permissions_and_seeding_limits_manage_to_admin_roles()
    {
        var method = typeof(CustomerSurvey.Api.Controllers.BranchesController).GetMethod("UpdateEvaluationSettings")!;
        var permission = method.GetCustomAttribute<CustomerSurvey.Api.Attribute.PermissionAttribute>()!;
        Assert.Equal("PERMISSION_Branches.ManageEvaluationSettings", permission.Policy);
        var id = CustomerSurvey.infrastructure.Seeders.SeedConstants.SeedIds.Permissions.BranchesManageEvaluationSettings;
        var grants = CustomerSurvey.infrastructure.Seeders.SeedConstants.SeedCatalog.RolePermissions.Where(x => x.PermissionId == id).ToArray();
        Assert.Equal(2, grants.Length);
        Assert.Contains(grants, x => x.RoleId == CustomerSurvey.infrastructure.Seeders.SeedConstants.SeedIds.Roles.BranchAdministrator);
        Assert.Contains(grants, x => x.RoleId == CustomerSurvey.infrastructure.Seeders.SeedConstants.SeedIds.Roles.SystemAdministrator);
    }
    internal static ResponseSnapshotDocument Document(params SnapshotQuestion[] questions) => new()
    {
        ResponseId = Guid.NewGuid(), TemplateId = Guid.NewGuid(), TemplateKind = 1, SubmittedOnUtc = DateTime.UtcNow,
        Provenance = SnapshotProvenance.CapturedAtSubmission, Questions = questions
    };
    internal static ResponseFlatDto Flat(ResponseSnapshotDocument document) => new()
    {
        ResponseId = document.ResponseId, TemplateId = document.TemplateId, TemplateKind = (ReportTemplateKind)document.TemplateKind,
        SnapshotPayload = ResponseSnapshot.Create(document).Payload, SubmittedOnUtc = document.SubmittedOnUtc
    };
    private sealed class Scope(Guid branch, DomainUserType actor, Guid user) : ICurrentBranchScopeResolver
    {
        public Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken ct = default) => Task.FromResult(Result<CurrentBranchScope>.Ok(
            new() { BranchId = branch, ActorType = actor, ApplicationUserId = user, ActorProfileId = Guid.NewGuid() }));
    }
    private sealed class SettingsUser(DomainUserType type) : ICurrentUser
    {
        public bool IsAuthenticated => true;
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid? AccountId => null;
        public Guid? ActiveBranchId => null;
        public string? Role => null;
        public BuildingBlock.Application.Abstraction.Security.UserType UserType => BuildingBlock.Application.Abstraction.Security.UserType.Unknown;
        public int? UserTypeValue => (int)type;
    }
}
