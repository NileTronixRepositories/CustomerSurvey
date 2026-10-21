namespace CustomerSurvey.Application.Abstraction.Reports;

public interface ITemplateAvailabilityReader
{
    Task<TemplateAvailabilityMetrics> ReadAsync(Guid? branchId, int? templateKind, Guid? templateId,
        DateTime fromUtc, DateTime toExclusiveUtc, CancellationToken cancellationToken);
}

public sealed record TemplateAvailabilityMetrics
{
    public int TemplatesWithResponsesCount { get; init; }
    public int AvailableTemplatesDuringPeriod { get; init; }
    public int CurrentlyAvailableTemplatesCount { get; init; }
    public bool IsHistoryComplete { get; init; }
    public DateTime? CoverageStartsOnUtc { get; init; }
}

public static class TemplateAvailabilityReaderExtensions
{
    public static async Task<TemplateAvailabilityMetrics> ReadTemplatesAsync(this ITemplateAvailabilityReader reader,
        IEnumerable<Guid> templateIds, DateTime fromUtc, DateTime toExclusiveUtc, CancellationToken ct)
    {
        var results = new List<TemplateAvailabilityMetrics>();
        foreach (var id in templateIds.Distinct())
            results.Add(await reader.ReadAsync(null, 1, id, fromUtc, toExclusiveUtc, ct));
        return new()
        {
            AvailableTemplatesDuringPeriod = results.Sum(x => x.AvailableTemplatesDuringPeriod),
            CurrentlyAvailableTemplatesCount = results.Sum(x => x.CurrentlyAvailableTemplatesCount),
            IsHistoryComplete = results.All(x => x.IsHistoryComplete),
            CoverageStartsOnUtc = results.Select(x => x.CoverageStartsOnUtc).Max()
        };
    }
}
