using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Infrastracture.MultiTenancy;
using BuildingBlock.Infrastracture.Presistence;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Persistence
{
    public class PlatformReadDbContext : DbContext
    {
        private readonly ICurrentTenantContext _tenantContext;

        // ✅ نفس الـ properties اللي الفلتر هيقرأ منها
        public Guid? CurrentAccountId => _tenantContext.AccountId;

        public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

        public PlatformReadDbContext(
            DbContextOptions<PlatformReadDbContext> options,
            ICurrentTenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyReadConfigurations(typeof(PlatformReadDbContext).Assembly);

            modelBuilder.ApplyTenantQueryFilters(this);

            base.OnModelCreating(modelBuilder);
        }
    }
}