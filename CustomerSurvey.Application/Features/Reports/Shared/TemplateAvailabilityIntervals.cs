using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Shared;

public static class TemplateAvailabilityIntervals
{
    public static bool WasAvailable(IEnumerable<TemplateAvailabilityEvent> events, DateTime fromUtc, DateTime toExclusiveUtc)
    {
        var ordered = events.OrderBy(x => x.ChangedOnUtc).ThenByDescending(x => x.IsBaseline).ThenBy(x => x.Id).ToArray();
        for (var i = 0; i < ordered.Length; i++)
        {
            var state = ordered[i];
            if (!state.IsActive) continue;
            var start = new[] { fromUtc, state.ChangedOnUtc, state.ActiveFrom }.Max();
            var end = new[] { toExclusiveUtc, state.ExpireTo ?? DateTime.MaxValue,
                i + 1 < ordered.Length ? ordered[i + 1].ChangedOnUtc : DateTime.MaxValue }.Min();
            if (start < end) return true;
        }
        return false;
    }
}
