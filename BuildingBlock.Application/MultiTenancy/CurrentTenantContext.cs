using BuildingBlock.Application.Abstraction.Security;

namespace BuildingBlock.Application.MultiTenancy
{
    public sealed class CurrentTenantContext : ICurrentTenantContext
    {
        public TenantMode Mode { get; internal set; } = TenantMode.Platform;

        public Guid? AccountId { get; internal set; }
        public Guid? UserId { get; internal set; }
        public bool IsAuthenticated { get; internal set; }

        public string? Role { get; internal set; }
        public UserType UserType { get; internal set; } = UserType.Unknown;

        public bool HasAccount => AccountId.HasValue && AccountId.Value != Guid.Empty;

        // ✅ Source of truth: UserType (with Role fallback if you want)
        public bool IsPlatformAdmin =>
            UserType is UserType.SuperAdmin or UserType.PlatformAdmin
            || string.Equals(Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Role, "PlatformAdmin", StringComparison.OrdinalIgnoreCase);
    }
}