using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Reports;

internal sealed class TemplateAvailabilityReader(IWriteReadRepository<Template> authorized,
    IWriteReadRepository<AnonymousTemplate> anonymous, IWriteReadRepository<TemplateAvailabilityEvent> history)
    : ITemplateAvailabilityReader
{
    public async Task<TemplateAvailabilityMetrics> ReadAsync(Guid? branchId, int? templateKind, Guid? templateId,
        DateTime fromUtc, DateTime toExclusiveUtc, CancellationToken ct)
    {
        var current = new List<State>();
        if (templateKind != 2)
            current.AddRange(await authorized.Query().AsNoTracking()
                .Where(x => (!branchId.HasValue || x.BranchId == branchId) && (!templateId.HasValue || x.Id == templateId))
                .Select(x => new State(x.Id, 1, x.IsActive, x.ActiveFrom, x.ExpireTo)).ToListAsync(ct));
        if (templateKind != 1)
            current.AddRange(await anonymous.Query().AsNoTracking()
                .Where(x => (!branchId.HasValue || x.BranchId == branchId) && (!templateId.HasValue || x.Id == templateId))
                .Select(x => new State(x.Id, 2, x.IsActive, x.ActiveFrom, x.ExpireTo)).ToListAsync(ct));
        var ids = current.Select(x => x.Id).Distinct().ToArray();
        var events = ids.Length == 0 ? new List<TemplateAvailabilityEvent>() : await history.Query().AsNoTracking()
            .Where(x => ids.Contains(x.TemplateId) && (!branchId.HasValue || x.BranchId == branchId))
            .ToListAsync(ct);
        var byTemplate = events.GroupBy(x => (x.TemplateKind, x.TemplateId)).ToDictionary(x => x.Key, x => x.ToArray());
        var now = DateTime.UtcNow;
        var complete = true;
        DateTime? coverage = null;
        var available = 0;
        foreach (var state in current)
        {
            var entries = byTemplate.GetValueOrDefault((state.Kind, state.Id), Array.Empty<TemplateAvailabilityEvent>());
            var first = entries.OrderBy(x => x.ChangedOnUtc).ThenByDescending(x => x.IsBaseline).FirstOrDefault();
            // A genuine creation establishes nonexistence before creation. A legacy baseline cannot.
            if (first is null || (first.IsBaseline && fromUtc < first.ChangedOnUtc)) complete = false;
            if (first?.IsBaseline == true && (!coverage.HasValue || first.ChangedOnUtc > coverage)) coverage = first.ChangedOnUtc;
            if (TemplateAvailabilityIntervals.WasAvailable(entries, fromUtc, toExclusiveUtc)) available++;
        }
        return new()
        {
            AvailableTemplatesDuringPeriod = available,
            CurrentlyAvailableTemplatesCount = current.Count(x => x.IsActive && x.From <= now && (!x.To.HasValue || x.To > now)),
            IsHistoryComplete = complete, CoverageStartsOnUtc = coverage
        };
    }
    private sealed record State(Guid Id, int Kind, bool IsActive, DateTime From, DateTime? To);
}
