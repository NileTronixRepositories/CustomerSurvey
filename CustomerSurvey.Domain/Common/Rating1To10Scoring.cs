namespace CustomerSurvey.Domain.Common;

public static class Rating1To10Scoring
{
    public static int? Normalize(int? ratingValue)
        => ratingValue is >= 1 and <= 10 ? (ratingValue.Value + 1) / 2 : null;
}
