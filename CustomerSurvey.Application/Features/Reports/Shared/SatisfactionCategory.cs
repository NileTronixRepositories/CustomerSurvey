namespace CustomerSurvey.Application.Features.Reports.Shared;

public enum SatisfactionCategory
{
    Satisfied = 1,
    Neutral = 2,
    Unhappy = 3
}

public static class SatisfactionCategoryRule
{
    public const decimal SatisfiedMinimum = 80m;
    public const decimal NeutralMinimum = 60m;

    public static bool IsScored(int maxScore) => maxScore > 0;

    public static SatisfactionCategory? Classify(int maxScore, decimal scorePercentage, EvaluationCriteria? criteria = null)
    {
        criteria ??= new EvaluationCriteria();
        if (!IsScored(maxScore))
        {
            return null;
        }

        if (scorePercentage >= criteria.SatisfiedMinimum)
        {
            return SatisfactionCategory.Satisfied;
        }

        return scorePercentage >= criteria.NeutralMinimum
            ? SatisfactionCategory.Neutral
            : SatisfactionCategory.Unhappy;
    }

    public static bool Matches(
        int maxScore,
        decimal scorePercentage,
        SatisfactionCategory category, EvaluationCriteria? criteria = null)
        => Classify(maxScore, scorePercentage, criteria) == category;
}
