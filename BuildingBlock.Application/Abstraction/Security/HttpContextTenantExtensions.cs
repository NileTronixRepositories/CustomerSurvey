using BuildingBlock.Application.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Application.Abstraction.Security
{
    public static class HttpContextTenantExtensions
    {
        public static Guid? GetUserId(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().UserId;

        public static Guid? GetAccountId(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().AccountId;

        public static TenantMode GetTenantMode(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().Mode;

        public static string? GetRole(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().Role;

        public static bool IsPlatformAdmin(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().IsPlatformAdmin;

        public static bool IsAccountRequest(this HttpContext http)
            => http.RequestServices.GetRequiredService<ICurrentTenantContext>().HasAccount;
    }
}