using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

internal sealed class SurveyReportScoringService : ISurveyReportScoringService
{
    private const decimal MaxScoreValue = 5m;

    public IReadOnlyCollection<CalculatedResponseScore> CalculateResponseScores(
        IReadOnlyCollection<ResponseFlatDto> responses,
        IReadOnlyCollection<AnswerFlatDto> answers,
        IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
        IReadOnlyCollection<ConditionFlatDto> conditions,
        IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
        ScoreCalculationMode scoreCalculationMode)
    {
        if (responses.Count == 0)
        {
            return Array.Empty<CalculatedResponseScore>();
        }

        var responseById = responses
            .GroupBy(x => new
            {
                x.TemplateKind,
                x.ResponseId
            })
            .ToDictionary(
                x => (x.Key.TemplateKind, x.Key.ResponseId),
                x => x.First());

        var scoreTokens = CalculateQuestionScoreTokens(
            responses,
            answers,
            templateQuestions,
            conditions,
            questionOptions,
            scoreCalculationMode);

        return scoreTokens
            .GroupBy(x => new
            {
                x.TemplateKind,
                x.ResponseId
            })
            .Select(group =>
            {
                var response = responseById[(group.Key.TemplateKind, group.Key.ResponseId)];
                var scoreValues = group.Select(x => x.ScoreValue).ToArray();
                var averageScoreValue = scoreValues.Average();

                return new CalculatedResponseScore
                {
                    ResponseId = response.ResponseId,
                    TemplateId = response.TemplateId,
                    TemplateKind = response.TemplateKind,
                    ScoredItemsCount = scoreValues.Length,
                    AverageScoreValue = ReportScoreRounding.Round(averageScoreValue),
                    ScorePercentage = ToScorePercentage(averageScoreValue)
                };
            })
            .ToArray();
    }

    public IReadOnlyCollection<QuestionScoreToken> CalculateQuestionScoreTokens(
        IReadOnlyCollection<ResponseFlatDto> responses,
        IReadOnlyCollection<AnswerFlatDto> answers,
        IReadOnlyCollection<TemplateQuestionFlatDto> templateQuestions,
        IReadOnlyCollection<ConditionFlatDto> conditions,
        IReadOnlyCollection<QuestionOptionFlatDto> questionOptions,
        ScoreCalculationMode scoreCalculationMode)
    {
        if (responses.Count == 0 || templateQuestions.Count == 0 || answers.Count == 0)
        {
            return Array.Empty<QuestionScoreToken>();
        }

        var templateQuestionsByTemplate = templateQuestions
            .GroupBy(x => x.TemplateId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var templateQuestionsById = templateQuestions
            .GroupBy(x => x.TemplateQuestionId)
            .ToDictionary(x => x.Key, x => x.First());

        var childTemplateQuestionIdsByTemplate = conditions
            .GroupBy(x => x.TemplateId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(c => c.ChildTemplateQuestionId).ToHashSet());

        var childConditionsByParent = conditions
            .GroupBy(x => x.ParentTemplateQuestionId)
            .ToDictionary(
                x => x.Key,
                x => x.OrderBy(c => GetChildQuestionOrder(c, templateQuestionsById)).ToArray());

        var answersByResponseAndQuestion = answers
            .GroupBy(x => new
            {
                x.ResponseId,
                x.TemplateId,
                x.QuestionId
            })
            .ToDictionary(
                x => (x.Key.ResponseId, x.Key.TemplateId, x.Key.QuestionId),
                x => x.First());

        var questionOptionsById = questionOptions
            .GroupBy(x => x.OptionId)
            .ToDictionary(x => x.Key, x => x.First());

        var result = new List<QuestionScoreToken>();

        foreach (var response in responses)
        {
            if (!templateQuestionsByTemplate.TryGetValue(response.TemplateId, out var currentTemplateQuestions))
            {
                continue;
            }

            if (!childTemplateQuestionIdsByTemplate.TryGetValue(response.TemplateId, out var childIds))
            {
                childIds = new HashSet<Guid>();
            }

            var rootQuestions = currentTemplateQuestions
                .Where(x => !childIds.Contains(x.TemplateQuestionId))
                .OrderBy(x => x.Order)
                .ToArray();

            foreach (var rootQuestion in rootQuestions)
            {
                if (scoreCalculationMode == ScoreCalculationMode.RootQuestions)
                {
                    AddRootScoreTokenIfPossible(
                        response,
                        rootQuestion,
                        answersByResponseAndQuestion,
                        questionOptionsById,
                        result);

                    continue;
                }

                AddLowestConditionLevelScoreTokenIfPossible(
                    response,
                    rootQuestion,
                    templateQuestionsById,
                    childConditionsByParent,
                    answersByResponseAndQuestion,
                    questionOptionsById,
                    result);
            }
        }

        return result;
    }

    private static void AddRootScoreTokenIfPossible(
        ResponseFlatDto response,
        TemplateQuestionFlatDto rootQuestion,
        IReadOnlyDictionary<(Guid ResponseId, Guid TemplateId, Guid QuestionId), AnswerFlatDto> answersByResponseAndQuestion,
        IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
        List<QuestionScoreToken> result)
    {
        if (!answersByResponseAndQuestion.TryGetValue(
                (response.ResponseId, response.TemplateId, rootQuestion.QuestionId),
                out var answer))
        {
            return;
        }

        if (!TryGetScoreValue(rootQuestion.QuestionType, answer, questionOptionsById, out var scoreValue))
        {
            return;
        }

        result.Add(BuildToken(response, rootQuestion, scoreValue));
    }

    private static void AddLowestConditionLevelScoreTokenIfPossible(
        ResponseFlatDto response,
        TemplateQuestionFlatDto rootQuestion,
        IReadOnlyDictionary<Guid, TemplateQuestionFlatDto> templateQuestionsById,
        IReadOnlyDictionary<Guid, ConditionFlatDto[]> childConditionsByParent,
        IReadOnlyDictionary<(Guid ResponseId, Guid TemplateId, Guid QuestionId), AnswerFlatDto> answersByResponseAndQuestion,
        IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
        List<QuestionScoreToken> result)
    {
        var current = rootQuestion;
        var visited = new HashSet<Guid>();

        QuestionScoreToken? lastScorableToken = null;

        while (true)
        {
            if (!visited.Add(current.TemplateQuestionId))
            {
                break;
            }

            if (!answersByResponseAndQuestion.TryGetValue(
                    (response.ResponseId, response.TemplateId, current.QuestionId),
                    out var currentAnswer))
            {
                break;
            }

            if (TryGetScoreValue(
                    current.QuestionType,
                    currentAnswer,
                    questionOptionsById,
                    out var scoreValue))
            {
                lastScorableToken = BuildToken(response, current, scoreValue);
            }

            if (!childConditionsByParent.TryGetValue(
                    current.TemplateQuestionId,
                    out var possibleConditions))
            {
                break;
            }

            var matchedCondition = possibleConditions.FirstOrDefault(condition =>
                IsConditionMatched(condition, currentAnswer));

            if (matchedCondition is null)
            {
                break;
            }

            if (!templateQuestionsById.TryGetValue(
                    matchedCondition.ChildTemplateQuestionId,
                    out var childQuestion))
            {
                break;
            }

            current = childQuestion;
        }

        if (lastScorableToken is not null)
        {
            result.Add(lastScorableToken);
        }
    }

    private static QuestionScoreToken BuildToken(
        ResponseFlatDto response,
        TemplateQuestionFlatDto templateQuestion,
        decimal scoreValue)
    {
        return new QuestionScoreToken
        {
            ResponseId = response.ResponseId,
            TemplateId = response.TemplateId,
            TemplateKind = response.TemplateKind,
            TemplateQuestionId = templateQuestion.TemplateQuestionId,
            QuestionId = templateQuestion.QuestionId,
            ScoreValue = scoreValue,
            ScorePercentage = ToScorePercentage(scoreValue)
        };
    }

    private static bool IsConditionMatched(
        ConditionFlatDto condition,
        AnswerFlatDto answer)
    {
        return condition.TriggerType switch
        {
            QuestionConditionTriggerType.SingleChoiceOption =>
                condition.SelectedQuestionOptionId.HasValue &&
                answer.SelectedQuestionOptionId.HasValue &&
                condition.SelectedQuestionOptionId.Value == answer.SelectedQuestionOptionId.Value,

            QuestionConditionTriggerType.StarRatingValue =>
                condition.TriggerValue.HasValue &&
                answer.StarRatingValue.HasValue &&
                condition.TriggerValue.Value == answer.StarRatingValue.Value,

            QuestionConditionTriggerType.SmileValue =>
                condition.TriggerValue.HasValue &&
                answer.SmileValue.HasValue &&
                condition.TriggerValue.Value == answer.SmileValue.Value,

            _ => false
        };
    }

    private static bool TryGetScoreValue(
        QuestionType questionType,
        AnswerFlatDto answer,
        IReadOnlyDictionary<Guid, QuestionOptionFlatDto> questionOptionsById,
        out decimal scoreValue)
    {
        scoreValue = 0;

        if (questionType == QuestionType.StarRating && answer.StarRatingValue.HasValue)
        {
            scoreValue = answer.StarRatingValue.Value;
            return true;
        }

        if (questionType == QuestionType.Smiles && answer.SmileValue.HasValue)
        {
            scoreValue = answer.SmileValue.Value;
            return true;
        }

        if (questionType == QuestionType.SingleChoice &&
            answer.SelectedQuestionOptionId.HasValue &&
            questionOptionsById.TryGetValue(answer.SelectedQuestionOptionId.Value, out var option))
        {
            scoreValue = option.Value;
            return true;
        }

        return false;
    }

    private static int GetChildQuestionOrder(
        ConditionFlatDto condition,
        IReadOnlyDictionary<Guid, TemplateQuestionFlatDto> templateQuestionsById)
        => templateQuestionsById.TryGetValue(condition.ChildTemplateQuestionId, out var child)
            ? child.Order
            : int.MaxValue;

    private static decimal ToScorePercentage(decimal scoreValue)
        => ReportScoreRounding.Round(scoreValue * 100m / MaxScoreValue);
}
