using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using UserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.FlagOfficers;

internal sealed record FlagOfficerAdministrationScope(Guid ApplicationUserId, Guid? DepartmentId)
{
    public bool Includes(Guid departmentId) => DepartmentId is null || DepartmentId == departmentId;
}

internal sealed class FlagOfficerAdministrationScopeResolver(
    ICurrentUser currentUser,
    IWriteReadRepository<SuperAdmin> superAdmins,
    IWriteReadRepository<DepartmentAdmin> departmentAdmins)
{
    internal static readonly Error Forbidden = new(
        "FlagOfficers.Forbidden", "This actor cannot administer the requested account.", ErrorType.Security);

    public async Task<Result<FlagOfficerAdministrationScope>> ResolveAsync(CancellationToken ct)
    {
        if (currentUser.IsAuthenticated && currentUser.UserId is { } userId)
        {
            if (await superAdmins.AnyAsync(x => x.ApplicationUserId == userId &&
                x.ApplicationUser.IsActive && x.ApplicationUser.UserType == UserType.SuperAdmin, ct))
            {
                return Result<FlagOfficerAdministrationScope>.Ok(new(userId, null));
            }

            var admin = await departmentAdmins.GetByPropertyAsync(x => x.ApplicationUserId == userId &&
                x.ApplicationUser.IsActive && x.ApplicationUser.UserType == UserType.DepartmentAdmin, ct);
            if (admin is not null)
            {
                return Result<FlagOfficerAdministrationScope>.Ok(new(userId, admin.DepartmentId));
            }
        }

        return Result<FlagOfficerAdministrationScope>.Fail(Forbidden);
    }
}
