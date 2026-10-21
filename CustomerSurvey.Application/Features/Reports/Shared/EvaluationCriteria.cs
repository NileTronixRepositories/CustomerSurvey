namespace CustomerSurvey.Application.Features.Reports.Shared;

public sealed record EvaluationCriteria
{
    public decimal NeutralMinimum { get; init; } = 60m;
    public decimal SatisfiedMinimum { get; init; } = 80m;
    public decimal MediumRiskMinimum { get; init; } = 50m;
    public decimal HealthyMinimum { get; init; } = 75m;

    public string Risk(decimal? average) => !average.HasValue ? "NoData" :
        average >= HealthyMinimum ? "Healthy" : average >= MediumRiskMinimum ? "MediumRisk" : "HighRisk";

    // Mixed-branch aggregates weight current branch criteria by their scored response contributions.
    public static EvaluationCriteria Combine(IEnumerable<EvaluationCriteria> scoredResponseCriteria)
    {
        var criteria = scoredResponseCriteria.ToArray();
        return criteria.Length == 0 ? new() : new()
        {
            NeutralMinimum = criteria.Average(x => x.NeutralMinimum),
            SatisfiedMinimum = criteria.Average(x => x.SatisfiedMinimum),
            MediumRiskMinimum = criteria.Average(x => x.MediumRiskMinimum),
            HealthyMinimum = criteria.Average(x => x.HealthyMinimum)
        };
    }
}

internal interface IEvaluatedResponse
{
    EvaluationCriteria Criteria { get; }
}
