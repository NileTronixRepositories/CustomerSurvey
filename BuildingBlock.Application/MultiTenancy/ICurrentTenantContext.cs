using BuildingBlock.Application.Abstraction.Security;

namespace BuildingBlock.Application.MultiTenancy
{
    public interface ICurrentTenantContext
    {
        TenantMode Mode { get; }
        Guid? AccountId { get; }
        Guid? UserId { get; }
        bool IsAuthenticated { get; }

        string? Role { get; }
        UserType UserType { get; }

        bool HasAccount { get; }
        bool IsPlatformAdmin { get; }
    }
}