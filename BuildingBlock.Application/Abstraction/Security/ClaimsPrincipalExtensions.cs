using System.Security.Claims;

namespace BuildingBlock.Application.Abstraction.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetClaimValue(this ClaimsPrincipal user, string claimType)
            => user?.FindFirst(claimType)?.Value;

        public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal user, string claimType)
            => user?.FindAll(claimType).Select(x => x.Value) ?? Enumerable.Empty<string>();
    }
}