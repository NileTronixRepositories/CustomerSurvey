using BuildingBlock.Application.MultiTenancy;
using Microsoft.EntityFrameworkCore;

using BuildingBlock.Infrastracture.Extensions;
using BuildingBlock.Infrastracture.MultiTenancy;
using BuildingBlock.Infrastracture.Presistence;

namespace CustomerSurvey.infrastructure.Persistence
{
    public class PlatformWriteDbContext : DbContext
    {
        private readonly ICurrentTenantContext _tenantContext;

        // ✅ دي اللي الفلتر هيعتمد عليها (per DbContext instance)
        public Guid? CurrentAccountId => _tenantContext.AccountId;

        public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

        public PlatformWriteDbContext(
            DbContextOptions<PlatformWriteDbContext> options,
            ICurrentTenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
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