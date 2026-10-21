using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Persistence;

// Explicit maintenance operation: bounded transactions, restartable selection, no migration-time rewrite.
public sealed class SurveyHistoryMaintenance(PlatformWriteDbContext db)
{
    public async Task<HistoryBackfillBatch> RunBatchAsync(int batchSize = 100, CancellationToken ct = default)
    {
        if (batchSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(batchSize));
        var responses = await db.Set<SurveyResponse>().Where(x => x.Snapshot == null)
            .OrderBy(x => x.Id).Take(batchSize).Include(x => x.Answers).ToListAsync(ct);
        foreach (var response in responses)
        {
            response.CaptureSnapshot(await ResponseSnapshotCapture.BuildAsync(db, response, null, true, ct));
            db.Add(response.Snapshot!);
        }
        var anonymous = await db.Set<AnonymousSurveyResponse>().Where(x => x.Snapshot == null)
            .OrderBy(x => x.Id).Take(batchSize).Include(x => x.Answers).ToListAsync(ct);
        foreach (var response in anonymous)
        {
            response.CaptureSnapshot(await ResponseSnapshotCapture.BuildAsync(db, null, response, true, ct));
            db.Add(response.Snapshot!);
        }
        var templates = await db.Set<Template>().Where(x => !db.Set<TemplateAvailabilityEvent>()
            .Any(e => e.TemplateKind == 1 && e.TemplateId == x.Id)).OrderBy(x => x.Id).Take(batchSize).ToListAsync(ct);
        var anonymousTemplates = await db.Set<AnonymousTemplate>().Where(x => !db.Set<TemplateAvailabilityEvent>()
            .Any(e => e.TemplateKind == 2 && e.TemplateId == x.Id)).OrderBy(x => x.Id).Take(batchSize).ToListAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var template in templates)
            db.Add(TemplateAvailabilityEvent.Create(template.Id, 1, template.BranchId, "BaselineInitialized",
                template.IsActive, template.ActiveFrom, template.ExpireTo, now, null, true));
        foreach (var template in anonymousTemplates)
            db.Add(TemplateAvailabilityEvent.Create(template.Id, 2, template.BranchId, "BaselineInitialized",
                template.IsActive, template.ActiveFrom, template.ExpireTo, now, null, true));
        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
        return new(responses.Count, anonymous.Count, templates.Count + anonymousTemplates.Count,
            responses.Count == batchSize || anonymous.Count == batchSize || templates.Count == batchSize || anonymousTemplates.Count == batchSize);
    }
}

public sealed record HistoryBackfillBatch(int AuthorizedSnapshots, int AnonymousSnapshots, int AvailabilityBaselines, bool MayHaveMore);
