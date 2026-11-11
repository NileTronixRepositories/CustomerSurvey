using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.FlagOfficers;

public sealed record FlagOfficerProfile(string NameEn, string? NameAr, string Email, string? PhoneNumber);

public sealed record CreateFlagOfficerCommand(
    Guid? DepartmentId, string UserName, string Password, FlagOfficerProfile Profile) : ICommand<FlagOfficerChange>;

public sealed record UpdateFlagOfficerCommand(Guid FlagOfficerId, FlagOfficerProfile Profile)
    : ICommand<FlagOfficerChange>;

public sealed record DeactivateFlagOfficerCommand(Guid FlagOfficerId) : ICommand<FlagOfficerChange>;
public sealed record RestoreFlagOfficerCommand(Guid FlagOfficerId) : ICommand<FlagOfficerChange>;
public sealed record AssignFlagOfficerRolesCommand(Guid FlagOfficerId, IReadOnlyCollection<Guid> RoleIds)
    : ICommand<IReadOnlyList<FlagCapabilityRole>>;

public sealed record SetOperatorFlagSubmissionCommand(Guid OperatorId, bool Enabled) : ICommand<bool>;

public sealed class GetFlagOfficersQuery : SearchParameters, IQuery<Pagination<FlagOfficerDetails>>
{
    public Guid? DepartmentId { get; init; }
}

public sealed record GetFlagOfficerQuery(Guid FlagOfficerId) : IQuery<FlagOfficerDetails>;
public sealed record GetFlagOfficerRolesQuery : IQuery<IReadOnlyList<FlagCapabilityRole>>;

public sealed record FlagOfficerChange(Guid FlagOfficerId, Guid ApplicationUserId, Guid DepartmentId, bool IsActive);
public sealed record FlagCapabilityRole(Guid RoleId, string Name, string Permission);

public sealed record FlagOfficerDetails(
    Guid FlagOfficerId, Guid ApplicationUserId, Guid DepartmentId,
    string DepartmentNameEn, string? DepartmentNameAr, string UserName, string Email,
    string NameEn, string? NameAr, string? PhoneNumber, bool IsActive,
    Guid CreatedByApplicationUserId, DateTime CreatedOnUtc, IReadOnlyList<FlagCapabilityRole> Roles);
