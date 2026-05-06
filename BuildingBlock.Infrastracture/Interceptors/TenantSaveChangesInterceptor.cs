using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Domain.Primitive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlock.Infrastracture.Interceptors
{
    public sealed class TenantSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentTenantContext _tenant;

        public TenantSaveChangesInterceptor(ICurrentTenantContext tenant)
        {
            _tenant = tenant;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyTenantRules(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyTenantRules(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyTenantRules(DbContext? context)
        {
            if (context is null) return;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is not ITenantEntity)
                    continue;

                // ✅ safe: أي write على Tenant entity لازم يكون فيه AccountId
                if (!_tenant.HasAccount)
                    throw new InvalidOperationException("محاولة كتابة بيانات Tenant بدون AccountId (Platform mode).");

                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(ITenantEntity.AccountId)).CurrentValue = _tenant.AccountId!.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    var accountProp = entry.Property(nameof(ITenantEntity.AccountId));
                    if (accountProp.IsModified)
                        throw new InvalidOperationException("AccountId لا يمكن تعديله بعد الإنشاء.");
                }
            }
        }
    }
}