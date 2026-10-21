using BuildingBlock.Application.MultiTenancy;
using Microsoft.EntityFrameworkCore;

using BuildingBlock.Infrastracture.Extensions;
using BuildingBlock.Infrastracture.MultiTenancy;
using BuildingBlock.Infrastracture.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Entities;
using BuildingBlock.Application.Abstraction.Security;

namespace CustomerSurvey.infrastructure.Persistence
{
    public class PlatformWriteDbContext : DbContext
    {
        private readonly ICurrentTenantContext _tenantContext;
        private readonly ICurrentUser? _currentUser;

        // ✅ دي اللي الفلتر هيعتمد عليها (per DbContext instance)
        public Guid? CurrentAccountId => _tenantContext.AccountId;

        public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

        public DbSet<BranchArea> BranchAreas => Set<BranchArea>();
        public DbSet<BranchAreaBranch> BranchAreaBranches => Set<BranchAreaBranch>();

        public PlatformWriteDbContext(
            DbContextOptions<PlatformWriteDbContext> options,
            ICurrentTenantContext tenantContext,
            ICurrentUser? currentUser = null)
            : base(options)
        {
            _tenantContext = tenantContext;
            _currentUser = currentUser;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PrepareHistoryAsync(CancellationToken.None).GetAwaiter().GetResult();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            await PrepareHistoryAsync(cancellationToken);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private async Task PrepareHistoryAsync(CancellationToken ct)
        {
            ChangeTracker.DetectChanges();
            if (ChangeTracker.Entries<ResponseSnapshot>().Any(x => x.State is EntityState.Modified or EntityState.Deleted) ||
                ChangeTracker.Entries<TemplateAvailabilityEvent>().Any(x => x.State is EntityState.Modified or EntityState.Deleted))
                throw new InvalidOperationException("Historical snapshots and availability events are immutable.");
            foreach (var entry in ChangeTracker.Entries().Where(x => x.Entity is SurveyResponse or AnonymousSurveyResponse))
                if (entry.State == EntityState.Modified && new[] { "ActualScore", "MaxScore", "ScorePercentage" }.Any(n => entry.Property(n).IsModified))
                    throw new InvalidOperationException("Original submission scores are immutable.");
            foreach (var entry in ChangeTracker.Entries<SurveyResponse>().Where(x => x.State == EntityState.Added).ToArray())
                if (entry.Entity.Snapshot is null)
                    entry.Entity.CaptureSnapshot(await ResponseSnapshotCapture.BuildAsync(this, entry.Entity, null, false, ct));
            foreach (var entry in ChangeTracker.Entries<AnonymousSurveyResponse>().Where(x => x.State == EntityState.Added).ToArray())
                if (entry.Entity.Snapshot is null)
                    entry.Entity.CaptureSnapshot(await ResponseSnapshotCapture.BuildAsync(this, null, entry.Entity, false, ct));
            foreach (var entry in ChangeTracker.Entries().Where(x => x.Entity is Template or AnonymousTemplate).ToArray())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified) continue;
                var added = entry.State == EntityState.Added;
                if (!added && !new[] { "IsActive", "ActiveFrom", "ExpireTo" }.Any(n => entry.Property(n).IsModified)) continue;
                var template = entry.Entity as Template;
                var anon = entry.Entity as AnonymousTemplate;
                var id = template?.Id ?? anon!.Id;
                var kind = template is not null ? 1 : 2;
                // A retried SaveChanges retains the pending history row.
                if (ChangeTracker.Entries<TemplateAvailabilityEvent>().Any(x => x.State == EntityState.Added &&
                    x.Entity.TemplateId == id && x.Entity.TemplateKind == kind && !x.Entity.IsBaseline)) continue;
                var now = DateTime.UtcNow;
                var branchId = template?.BranchId ?? anon?.BranchId;
                var active = template?.IsActive ?? anon!.IsActive;
                var from = template?.ActiveFrom ?? anon!.ActiveFrom;
                var to = template is not null ? template.ExpireTo : anon!.ExpireTo;
                var previous = added ? null : await entry.GetDatabaseValuesAsync(ct);
                bool? oldActive = added ? null : (bool?)previous?["IsActive"];
                DateTime? oldFrom = added ? null : (DateTime?)previous?["ActiveFrom"];
                DateTime? oldTo = added ? null : (DateTime?)previous?["ExpireTo"];
                if (!added && previous is not null && oldActive == active && oldFrom == from && oldTo == to) continue;
                if (!added && previous is not null && !await Set<TemplateAvailabilityEvent>().AnyAsync(x => x.TemplateId == id && x.TemplateKind == kind, ct))
                    Add(TemplateAvailabilityEvent.Create(id, kind, branchId, "BaselineInitialized", oldActive!.Value,
                        oldFrom!.Value, oldTo, now, _currentUser?.UserId, true));
                var eventType = added ? (anon?.SourceGlobalAnonymousTemplateId.HasValue == true ? "AssignedToBranch" :
                    template?.OriginTemplateId.HasValue == true || anon?.OriginAnonymousTemplateId.HasValue == true || template?.TemplateFamilyId.HasValue == true || anon?.TemplateFamilyId.HasValue == true ? "Copied" : "Created") : active != oldActive ? (active ? "Activated" : "Deactivated") : "ScheduleChanged";
                Add(TemplateAvailabilityEvent.Create(id, kind, branchId, eventType, active, from, to, now,
                    _currentUser?.UserId ?? (added ? template?.CreatedByApplicationUserId ?? anon?.CreatedByApplicationUserId : null),
                    false, oldActive, oldFrom, oldTo));
            }
            ChangeTracker.DetectChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyWriteConfigurations(typeof(PlatformWriteDbContext).Assembly);
            modelBuilder.ApplySoftDeleteQueryFilter();

            modelBuilder.ApplyTenantQueryFilters(this);

            base.OnModelCreating(modelBuilder);
        }
    }
}
