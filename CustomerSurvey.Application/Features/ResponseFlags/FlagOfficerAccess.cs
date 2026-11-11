using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Shared;
using CustomerSurvey.Domain.Identity;
using UserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.ResponseFlags;

internal sealed class FlagOfficerAccess(
    ICurrentUser currentUser, IWriteReadRepository<FlagOfficer> officers, FlagPermissions permissions)
{
    public async Task<Result<Guid>> RequireAsync(string permission, CancellationToken ct)
    {
        if (currentUser.IsAuthenticated && currentUser.UserId is { } userId &&
            await officers.AnyAsync(x => x.ApplicationUserId == userId, ct) &&
            await permissions.HasAsync(userId, UserType.FlagOfficer, permission, ct))
        {
            return Result<Guid>.Ok(userId);
        }

        return Result<Guid>.Fail(new Error(permission,
            "An active Flag Officer with this permission is required.", ErrorType.Security));
    }
}
