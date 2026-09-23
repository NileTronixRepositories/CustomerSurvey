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

    public static SatisfactionCategory? Classify(int maxScore, decimal scorePercentage)
    {
        if (!IsScored(maxScore))
        {
            return null;
        }

        if (scorePercentage >= SatisfiedMinimum)
        {
            return SatisfactionCategory.Satisfied;
        }

        return scorePercentage >= NeutralMinimum
            ? SatisfactionCategory.Neutral
            : SatisfactionCategory.Unhappy;
    }

    public static bool Matches(
        int maxScore,
        decimal scorePercentage,
        SatisfactionCategory category)
        => Classify(maxScore, scorePercentage) == category;
}
