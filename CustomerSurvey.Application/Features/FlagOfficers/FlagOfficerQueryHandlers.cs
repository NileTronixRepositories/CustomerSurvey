using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.FlagOfficers;

internal sealed class FlagOfficerQueryHandlers(
    FlagOfficerAdministrationScopeResolver scopeResolver,
    IWriteReadRepository<FlagOfficer> officers,
    IWriteReadRepository<RolePermission> rolePermissions) :
    IQueryHandler<GetFlagOfficersQuery, Pagination<FlagOfficerDetails>>,
    IQueryHandler<GetFlagOfficerQuery, FlagOfficerDetails>,
    IQueryHandler<GetFlagOfficerRolesQuery, IReadOnlyList<FlagCapabilityRole>>
{
    public async Task<Result<Pagination<FlagOfficerDetails>>> Handle(GetFlagOfficersQuery request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure)
            return Result<Pagination<FlagOfficerDetails>>.Fail(scope.Errors);
        if (request.DepartmentId is { } departmentId && !scope.Value.Includes(departmentId))
            return Result<Pagination<FlagOfficerDetails>>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        var (officerList, count) = await officers.ListWithCountAsync(
            new FlagOfficerDetailsSpec(request, scope.Value.DepartmentId ?? request.DepartmentId), ct);
        return Result<Pagination<FlagOfficerDetails>>.Ok(new(request.PageNumber, request.PageSize, count, officerList));
    }

    public async Task<Result<FlagOfficerDetails>> Handle(GetFlagOfficerQuery request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        if (scope.IsFailure)
            return Result<FlagOfficerDetails>.Fail(scope.Errors);

        var officer = await officers.FirstOrDefaultAsync(new FlagOfficerDetailsSpec(request.FlagOfficerId), ct);
        if (officer is null)
            return Result<FlagOfficerDetails>.Fail(FlagOfficerErrors.NotFound);
        if (!scope.Value.Includes(officer.DepartmentId))
            return Result<FlagOfficerDetails>.Fail(FlagOfficerAdministrationScopeResolver.Forbidden);

        return Result<FlagOfficerDetails>.Ok(officer);
    }

    public async Task<Result<IReadOnlyList<FlagCapabilityRole>>> Handle(GetFlagOfficerRolesQuery request, CancellationToken ct)
    {
        var scope = await scopeResolver.ResolveAsync(ct);
        return scope.IsFailure
            ? Result<IReadOnlyList<FlagCapabilityRole>>.Fail(scope.Errors)
            : Result<IReadOnlyList<FlagCapabilityRole>>.Ok(await rolePermissions.ListAsync(new FlagOfficerCapabilityRolesSpec(), ct));
    }
}
