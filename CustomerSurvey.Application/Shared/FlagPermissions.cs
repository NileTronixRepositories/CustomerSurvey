using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Shared;

internal sealed class FlagPermissions(IWriteReadRepository<UserRole> userRoles)
{
    public Task<bool> HasAsync(Guid userId, UserType userType, string permission, CancellationToken ct)
        => userRoles.AnyAsync(x => x.ApplicationUserId == userId &&
            x.ApplicationUser.UserType == userType && x.ApplicationUser.IsActive &&
            x.Role.RolePermissions.Any(grant => grant.Permission.Name == permission), ct);
}
