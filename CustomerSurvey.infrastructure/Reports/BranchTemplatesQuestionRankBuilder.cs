using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.infrastructure.Reports;

internal static class BranchTemplatesQuestionRankBuilder
{
    private const decimal MaxScoreValue = 5m;

    public static IReadOnlyCollection<BranchTemplatesPdfQuestionRankItem> Build(
        IReadOnlyCollection<BranchTemplatesPdfQuestionAnalytics> questions,
        int count,
        bool worst,
        decimal worstQuestionsMaxScorePercentage,
        decimal bestQuestionsMinScorePercentage)
    {
        var scoredQuestions = questions
            .Where(x => x.IsScoreIncluded && x.ScoreAverageValue.HasValue)
            .Select(x => new
            {
                Question = x,
                SatisfactionPercentage = Math.Round(
                    x.ScoreAverageValue!.Value * 100m / MaxScoreValue,
                    2)
            })
            .ToArray();

        var ordered = worst
            ? scoredQuestions
                .Where(x => x.SatisfactionPercentage <= worstQuestionsMaxScorePercentage)
                .OrderBy(x => x.SatisfactionPercentage)
                .ThenByDescending(x => x.Question.TotalAnswers)
                .ThenBy(x => x.Question.QuestionTextEn)
            : scoredQuestions
                .Where(x => x.SatisfactionPercentage >= bestQuestionsMinScorePercentage)
                .OrderByDescending(x => x.SatisfactionPercentage)
                .ThenByDescending(x => x.Question.TotalAnswers)
                .ThenBy(x => x.Question.QuestionTextEn);

        return ordered
            .Take(count)
            .Select((rankedQuestion, index) =>
            {
                var question = rankedQuestion.Question;

                return new BranchTemplatesPdfQuestionRankItem
                {
                    Rank = index + 1,
                    TemplateId = question.TemplateId,
                    TemplateKind = question.TemplateKind,
                    TemplateNameEn = question.TemplateNameEn,
                    TemplateNameAr = question.TemplateNameAr,
                    TemplateQuestionId = question.TemplateQuestionId,
                    QuestionTextEn = question.QuestionTextEn,
                    QuestionTextAr = question.QuestionTextAr,
                    IsRootQuestion = question.IsRootQuestion,
                    QuestionType = question.QuestionType,
                    TotalAnswers = question.TotalAnswers,
                    AverageScoreValue = question.ScoreAverageValue!.Value,
                    SatisfactionPercentage = rankedQuestion.SatisfactionPercentage
                };
            })
            .ToArray();
    }
}
