using System.Data;
using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using UserType = CustomerSurvey.Domain.Enums.UserType;

namespace CustomerSurvey.Application.Features.FlagOfficers;

internal sealed class CreateFlagOfficerHandler(
    FlagOfficerAdministrationScopeResolver scopeResolver,
    IWriteReadRepository<Department> departments,
    IWriteReadRepository<ApplicationUser> users,
    IWriteReadRepository<Role> roles,
    IWriteRepository<ApplicationUser> userWriter,
    IWriteRepository<FlagOfficer> officerWriter,
    IWriteRepository<UserRole> roleWriter,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateFlagOfficerCommand, FlagOfficerChange>
{
    public async Task<Result<FlagOfficerChange>> Handle(CreateFlagOfficerCommand request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure)
            return Result<FlagOfficerChange>.Fail(scope.Errors);

        if (request.DepartmentId is { } requestedDepartment && !scope.Value.Includes(requestedDepartment))
            return Result<FlagOfficerChange>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        if ((scope.Value.DepartmentId ?? request.DepartmentId) is not { } departmentId)
            return Result<FlagOfficerChange>.Fail(new Error(
                "FlagOfficers.DepartmentRequired", "A department is required.", ErrorType.Validation));

        if (!await departments.AnyAsync(x => x.Id == departmentId && x.IsActive, ct))
            return Result<FlagOfficerChange>.Fail(new Error(
                "FlagOfficers.DepartmentNotFound", "Active department not found.", ErrorType.NotFound));

        var userName = request.UserName.Trim();
        var email = request.Profile.Email.Trim().ToLowerInvariant();
        if (await users.AnyAsync(x => x.UserName == userName, ct))
            return Result<FlagOfficerChange>.Fail(new Error(
                "FlagOfficers.UserNameAlreadyExists", "The username already exists.", ErrorType.Validation));
        if (await users.AnyAsync(x => x.Email == email, ct))
            return Result<FlagOfficerChange>.Fail(FlagOfficerErrors.DuplicateEmail);

        var role = await roles.GetByPropertyAsync(x => x.IsSystemRole && x.Name == FlagAccess.FlagOfficerRole, ct);
        if (role is null)
            return Result<FlagOfficerChange>.Fail(new Error(
                "FlagOfficers.RoleNotFound", "Flag Officer role not found.", ErrorType.NotFound));

        var actorId = scope.Value.ApplicationUserId;
        var hash = new PasswordHasher<ApplicationUser>().HashPassword(null!, request.Password);
        var user = ApplicationUser.Create(userName, email, request.Profile.NameEn, request.Profile.NameAr,
            request.Profile.PhoneNumber, hash, UserType.FlagOfficer, actorId);
        var officer = FlagOfficer.Create(user.Id, departmentId, actorId);
        await userWriter.AddAsync(user, ct);
        await officerWriter.AddAsync(officer, ct);
        await roleWriter.AddAsync(UserRole.Create(user.Id, role.Id, actorId), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<FlagOfficerChange>.Ok(new(officer.Id, user.Id, departmentId, user.IsActive));
    }
}

internal sealed class ChangeFlagOfficerHandler(
    FlagOfficerAdministrationScopeResolver scopeResolver,
    IWriteReadRepository<FlagOfficer> officers,
    IWriteReadRepository<ApplicationUser> users,
    IUnitOfWork unitOfWork) :
    ICommandHandler<UpdateFlagOfficerCommand, FlagOfficerChange>,
    ICommandHandler<DeactivateFlagOfficerCommand, FlagOfficerChange>,
    ICommandHandler<RestoreFlagOfficerCommand, FlagOfficerChange>
{
    public Task<Result<FlagOfficerChange>> Handle(UpdateFlagOfficerCommand request, CancellationToken ct)
        => ChangeAsync(request.FlagOfficerId, request.Profile, null, ct);

    public Task<Result<FlagOfficerChange>> Handle(DeactivateFlagOfficerCommand request, CancellationToken ct)
        => ChangeAsync(request.FlagOfficerId, null, false, ct);

    public Task<Result<FlagOfficerChange>> Handle(RestoreFlagOfficerCommand request, CancellationToken ct)
        => ChangeAsync(request.FlagOfficerId, null, true, ct);

    private async Task<Result<FlagOfficerChange>> ChangeAsync(
        Guid officerId, FlagOfficerProfile? profile, bool? isActive, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure)
            return Result<FlagOfficerChange>.Fail(scope.Errors);

        var officer = await officers.GetByPropertyAsync(x => x.Id == officerId, ct);
        if (officer is null)
            return Result<FlagOfficerChange>.Fail(FlagOfficerErrors.NotFound);
        if (!scope.Value.Includes(officer.DepartmentId))
            return Result<FlagOfficerChange>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        var user = await users.GetByPropertyTrackedAsync(
            x => x.Id == officer.ApplicationUserId && x.UserType == UserType.FlagOfficer, ct);
        if (user is null)
            return Result<FlagOfficerChange>.Fail(FlagOfficerErrors.NotFound);

        if (profile is not null)
        {
            var email = profile.Email.Trim().ToLowerInvariant();
            if (await users.AnyAsync(x => x.Id != user.Id && x.Email == email, ct))
                return Result<FlagOfficerChange>.Fail(FlagOfficerErrors.DuplicateEmail);

            user.UpdateProfile(profile.NameEn, profile.NameAr, email, profile.PhoneNumber);
        }

        if (isActive == true)
            user.Activate();
        else if (isActive == false)
            user.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
        return Result<FlagOfficerChange>.Ok(new(officer.Id, user.Id, officer.DepartmentId, user.IsActive));
    }
}

internal sealed class AssignFlagOfficerRolesHandler(
    FlagOfficerAdministrationScopeResolver scopeResolver,
    IWriteReadRepository<FlagOfficer> officers,
    IWriteReadRepository<RolePermission> rolePermissions,
    IWriteReadRepository<UserRole> userRoles,
    IWriteRepository<UserRole> writer,
    IUnitOfWork unitOfWork) : ICommandHandler<AssignFlagOfficerRolesCommand, IReadOnlyList<FlagCapabilityRole>>
{
    public async Task<Result<IReadOnlyList<FlagCapabilityRole>>> Handle(AssignFlagOfficerRolesCommand request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure)
            return Result<IReadOnlyList<FlagCapabilityRole>>.Fail(scope.Errors);

        var officer = await officers.GetByPropertyAsync(x => x.Id == request.FlagOfficerId, ct);
        if (officer is null)
            return Result<IReadOnlyList<FlagCapabilityRole>>.Fail(FlagOfficerErrors.NotFound);
        if (!scope.Value.Includes(officer.DepartmentId))
            return Result<IReadOnlyList<FlagCapabilityRole>>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        var requested = request.RoleIds.ToHashSet();
        var available = await rolePermissions.ListAsync(new FlagOfficerCapabilityRolesSpec(), ct);
        var selected = available.Where(role => requested.Contains(role.RoleId)).ToArray();
        if (selected.Length != requested.Count)
            return Result<IReadOnlyList<FlagCapabilityRole>>.Fail(new Error(
                "FlagOfficers.RoleNotAllowed", "Only Flag Officer capability roles can be assigned.", ErrorType.Security));

        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await userRoles.ListAsync(new CapabilityAssignmentsSpec(officer.ApplicationUserId), ct);
        writer.DeleteRange(existing.Where(role => !requested.Contains(role.RoleId)));
        var existingIds = existing.Select(role => role.RoleId).ToHashSet();
        foreach (var role in selected.Where(role => !existingIds.Contains(role.RoleId)))
            await writer.AddAsync(UserRole.Create(officer.ApplicationUserId, role.RoleId, scope.Value.ApplicationUserId), ct);

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result<IReadOnlyList<FlagCapabilityRole>>.Ok(selected);
    }
}

internal sealed class SetOperatorFlagSubmissionHandler(
    FlagOfficerAdministrationScopeResolver scopeResolver,
    IWriteReadRepository<Operator> operators,
    IWriteReadRepository<Role> roles,
    IWriteReadRepository<UserRole> userRoles,
    IWriteRepository<UserRole> writer,
    IUnitOfWork unitOfWork) : ICommandHandler<SetOperatorFlagSubmissionCommand, bool>
{
    public async Task<Result<bool>> Handle(SetOperatorFlagSubmissionCommand request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure || scope.Value.DepartmentId.HasValue)
            return Result<bool>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        var target = await operators.GetByPropertyAsync(x => x.Id == request.OperatorId &&
            x.ApplicationUser.UserType == UserType.Operator, ct);
        if (target is null)
            return Result<bool>.Fail(new Error("Operators.NotFound", "Operator not found.", ErrorType.NotFound));

        var role = await roles.GetByPropertyAsync(x => x.IsSystemRole && x.Name == FlagAccess.CreateOnSubmitRole, ct);
        if (role is null)
            return Result<bool>.Fail(new Error(
                "Operators.FlagSubmissionRoleNotFound", "Submit Response Flags role not found.", ErrorType.NotFound));

        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await userRoles.GetByPropertyTrackedAsync(
            x => x.ApplicationUserId == target.ApplicationUserId && x.RoleId == role.Id, ct);
        if (request.Enabled && existing is null)
            await writer.AddAsync(UserRole.Create(target.ApplicationUserId, role.Id, scope.Value.ApplicationUserId), ct);
        else if (!request.Enabled && existing is not null)
            writer.Delete(existing);

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result<bool>.Ok(request.Enabled);
    }
}

internal static class FlagOfficerErrors
{
    public static readonly Error NotFound = new("FlagOfficers.NotFound", "Flag Officer not found.", ErrorType.NotFound);
    public static readonly Error DuplicateEmail = new(
        "FlagOfficers.EmailAlreadyExists", "The email address already exists.", ErrorType.Validation);
}
