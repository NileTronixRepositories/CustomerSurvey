using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Domain.Enums;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class SurveyReportScoringServiceTests
{
    private readonly SurveyReportScoringService _sut = new();

    [Fact]
    public void CalculateResponseScores_RootQuestions_UsesRootScorableQuestionsOnly()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var questions = new[]
        {
            Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating),
            Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.Smiles),
            Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 3, QuestionType.SingleChoice)
        };

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, questions[0].QuestionId, QuestionType.StarRating, star: 5),
                Answer(responseId, templateId, questions[1].QuestionId, QuestionType.Smiles, smile: 4),
                Answer(responseId, templateId, questions[2].QuestionId, QuestionType.SingleChoice, optionId: optionId)
            },
            questions,
            Array.Empty<ConditionFlatDto>(),
            new[] { Option(optionId, questions[2].QuestionId, 3) },
            ScoreCalculationMode.RootQuestions);

        var score = Assert.Single(scores);
        Assert.Equal(3, score.ScoredItemsCount);
        Assert.Equal(4m, score.AverageScoreValue);
        Assert.Equal(80m, score.ScorePercentage);
    }

    [Fact]
    public void CalculateResponseScores_RootQuestions_IgnoresConditionalChildQuestions()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var root = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating);
        var child = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.StarRating);

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, root.QuestionId, QuestionType.StarRating, star: 5),
                Answer(responseId, templateId, child.QuestionId, QuestionType.StarRating, star: 1)
            },
            new[] { root, child },
            new[] { Condition(templateId, root.TemplateQuestionId, child.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, triggerValue: 5) },
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.RootQuestions);

        var score = Assert.Single(scores);
        Assert.Equal(1, score.ScoredItemsCount);
        Assert.Equal(100m, score.ScorePercentage);
    }

    [Fact]
    public void CalculateResponseScores_RootQuestions_IgnoresNonScorableQuestions()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var star = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating);
        var complaint = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.Complain);
        var voice = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 3, QuestionType.Voice);

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, star.QuestionId, QuestionType.StarRating, star: 5),
                Answer(responseId, templateId, complaint.QuestionId, QuestionType.Complain, hasText: true),
                Answer(responseId, templateId, voice.QuestionId, QuestionType.Voice, hasVoice: true)
            },
            new[] { star, complaint, voice },
            Array.Empty<ConditionFlatDto>(),
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.RootQuestions);

        var score = Assert.Single(scores);
        Assert.Equal(1, score.ScoredItemsCount);
        Assert.Equal(100m, score.ScorePercentage);
    }

    [Fact]
    public void CalculateResponseScores_LowestConditionLevel_UsesDeepestScorableChild()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var root = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating);
        var child = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.StarRating);

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, root.QuestionId, QuestionType.StarRating, star: 5),
                Answer(responseId, templateId, child.QuestionId, QuestionType.StarRating, star: 2)
            },
            new[] { root, child },
            new[] { Condition(templateId, root.TemplateQuestionId, child.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, triggerValue: 5) },
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.LowestConditionLevel);

        var score = Assert.Single(scores);
        Assert.Equal(40m, score.ScorePercentage);
    }

    [Fact]
    public void CalculateQuestionScoreTokens_LowestConditionLevel_FallsBackWhenDeepestQuestionIsNonScorable()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var root = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating);
        var child = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.StarRating);
        var complaint = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 3, QuestionType.Complain);

        var tokens = _sut.CalculateQuestionScoreTokens(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, root.QuestionId, QuestionType.StarRating, star: 4),
                Answer(responseId, templateId, child.QuestionId, QuestionType.StarRating, star: 2),
                Answer(responseId, templateId, complaint.QuestionId, QuestionType.Complain, hasText: true)
            },
            new[] { root, child, complaint },
            new[]
            {
                Condition(templateId, root.TemplateQuestionId, child.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, triggerValue: 4),
                Condition(templateId, child.TemplateQuestionId, complaint.TemplateQuestionId, QuestionConditionTriggerType.StarRatingValue, triggerValue: 2)
            },
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.LowestConditionLevel);

        var token = Assert.Single(tokens);
        Assert.Equal(child.TemplateQuestionId, token.TemplateQuestionId);
        Assert.Equal(2m, token.ScoreValue);
        Assert.Equal(40m, token.ScorePercentage);
    }

    [Fact]
    public void CalculateResponseScores_ExcludesResponsesWithNoScorableAnswers()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var complaint = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.Complain);
        var voice = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.Voice);

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[]
            {
                Answer(responseId, templateId, complaint.QuestionId, QuestionType.Complain, hasText: true),
                Answer(responseId, templateId, voice.QuestionId, QuestionType.Voice, hasVoice: true)
            },
            new[] { complaint, voice },
            Array.Empty<ConditionFlatDto>(),
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.RootQuestions);

        Assert.Empty(scores);
    }

    [Fact]
    public void CalculateResponseScores_SingleChoice_UsesSelectedOptionValue()
    {
        var templateId = Guid.NewGuid();
        var responseId = Guid.NewGuid();
        var question = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.SingleChoice);
        var optionId = Guid.NewGuid();

        var scores = _sut.CalculateResponseScores(
            new[] { Response(responseId, templateId) },
            new[] { Answer(responseId, templateId, question.QuestionId, QuestionType.SingleChoice, optionId: optionId) },
            new[] { question },
            Array.Empty<ConditionFlatDto>(),
            new[] { Option(optionId, question.QuestionId, 4) },
            ScoreCalculationMode.RootQuestions);

        var score = Assert.Single(scores);
        Assert.Equal(80m, score.ScorePercentage);
    }

    [Fact]
    public void CalculateResponseScores_AllowsOverallAverageToBeWeightedByResponse()
    {
        var templateId = Guid.NewGuid();
        var firstResponseId = Guid.NewGuid();
        var secondResponseId = Guid.NewGuid();
        var firstQuestion = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 1, QuestionType.StarRating);
        var secondQuestion = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 2, QuestionType.StarRating);
        var thirdQuestion = Question(templateId, Guid.NewGuid(), Guid.NewGuid(), 3, QuestionType.StarRating);

        var scores = _sut.CalculateResponseScores(
            new[]
            {
                Response(firstResponseId, templateId),
                Response(secondResponseId, templateId)
            },
            new[]
            {
                Answer(firstResponseId, templateId, firstQuestion.QuestionId, QuestionType.StarRating, star: 5),
                Answer(secondResponseId, templateId, secondQuestion.QuestionId, QuestionType.StarRating, star: 2),
                Answer(secondResponseId, templateId, thirdQuestion.QuestionId, QuestionType.StarRating, star: 3)
            },
            new[] { firstQuestion, secondQuestion, thirdQuestion },
            Array.Empty<ConditionFlatDto>(),
            Array.Empty<QuestionOptionFlatDto>(),
            ScoreCalculationMode.RootQuestions);

        Assert.Equal(75m, scores.Average(x => x.ScorePercentage));
    }

    private static ResponseFlatDto Response(Guid responseId, Guid templateId)
        => new()
        {
            ResponseId = responseId,
            TemplateId = templateId,
            TemplateKind = ReportTemplateKind.Normal,
            SubmittedOnUtc = DateTime.UtcNow
        };

    private static TemplateQuestionFlatDto Question(
        Guid templateId,
        Guid templateQuestionId,
        Guid questionId,
        int order,
        QuestionType questionType)
        => new()
        {
            TemplateId = templateId,
            TemplateQuestionId = templateQuestionId,
            QuestionId = questionId,
            Order = order,
            QuestionTextEn = questionType.ToString(),
            QuestionType = questionType
        };

    private static ConditionFlatDto Condition(
        Guid templateId,
        Guid parentTemplateQuestionId,
        Guid childTemplateQuestionId,
        QuestionConditionTriggerType triggerType,
        Guid? selectedQuestionOptionId = null,
        int? triggerValue = null)
        => new()
        {
            TemplateId = templateId,
            ParentTemplateQuestionId = parentTemplateQuestionId,
            ChildTemplateQuestionId = childTemplateQuestionId,
            TriggerType = triggerType,
            SelectedQuestionOptionId = selectedQuestionOptionId,
            TriggerValue = triggerValue
        };

    private static AnswerFlatDto Answer(
        Guid responseId,
        Guid templateId,
        Guid questionId,
        QuestionType questionType,
        Guid? optionId = null,
        int? star = null,
        int? smile = null,
        bool hasText = false,
        bool hasVoice = false)
        => new()
        {
            ResponseId = responseId,
            TemplateId = templateId,
            QuestionId = questionId,
            QuestionType = questionType,
            SelectedQuestionOptionId = optionId,
            StarRatingValue = star,
            SmileValue = smile,
            HasTextAnswer = hasText,
            HasVoiceAnswer = hasVoice
        };

    private static QuestionOptionFlatDto Option(
        Guid optionId,
        Guid questionId,
        int value)
        => new()
        {
            OptionId = optionId,
            QuestionId = questionId,
            TextEn = value.ToString(),
            Value = value
        };
}
