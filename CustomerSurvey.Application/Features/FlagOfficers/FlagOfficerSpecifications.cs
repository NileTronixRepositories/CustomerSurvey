using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.FlagOfficers;

internal sealed class FlagOfficerDetailsSpec : Specification<FlagOfficer, FlagOfficerDetails>
{
    public FlagOfficerDetailsSpec(Guid flagOfficerId) : this()
        => AddCriteria(x => x.Id == flagOfficerId);

    public FlagOfficerDetailsSpec(GetFlagOfficersQuery query, Guid? departmentId) : this()
    {
        if (departmentId is { } department)
            AddCriteria(x => x.DepartmentId == department);

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var search = query.SearchText.Trim();
            AddCriteria(x => x.ApplicationUser.UserName.Contains(search) ||
                x.ApplicationUser.Email.Contains(search) ||
                x.ApplicationUser.NameEn.Contains(search) ||
                (x.ApplicationUser.NameAr != null && x.ApplicationUser.NameAr.Contains(search)) ||
                x.Department.NameEn.Contains(search) ||
                (x.Department.NameAr != null && x.Department.NameAr.Contains(search)));
        }

        if (query.OrderSort == OrderSort.Newest)
        {
            AddOrderByDescending(x => x.CreatedOnUtc);
            AddOrderByDescending(x => x.Id);
        }
        else
        {
            AddOrderBy(x => x.CreatedOnUtc);
            AddOrderBy(x => x.Id);
        }

        EnableTotalCount();
        ApplyPaging(query.PageNumber, query.PageSize);
    }

    private FlagOfficerDetailsSpec()
    {
        UseNoTracking();
        Select(x => new FlagOfficerDetails(x.Id, x.ApplicationUserId, x.DepartmentId,
            x.Department.NameEn, x.Department.NameAr, x.ApplicationUser.UserName, x.ApplicationUser.Email,
            x.ApplicationUser.NameEn, x.ApplicationUser.NameAr, x.ApplicationUser.PhoneNumber,
            x.ApplicationUser.IsActive, x.CreatedByApplicationUserId, x.CreatedOnUtc,
            x.ApplicationUser.UserRoles.Where(link => link.Role.IsSystemRole &&
                    (link.Role.Name == FlagAccess.ViewRole || link.Role.Name == FlagAccess.CreateRole ||
                     link.Role.Name == FlagAccess.AnswerRole || link.Role.Name == FlagAccess.EditBriefRole))
                .OrderBy(link => link.Role.Name)
                .Select(link => new FlagCapabilityRole(link.RoleId, link.Role.Name,
                    link.Role.RolePermissions.Select(grant => grant.Permission.Name).FirstOrDefault() ?? string.Empty))
                .ToList()));
    }
}

internal sealed class FlagOfficerCapabilityRolesSpec : Specification<RolePermission, FlagCapabilityRole>
{
    public FlagOfficerCapabilityRolesSpec()
    {
        AddCriteria(x => x.Role.IsSystemRole &&
            ((x.Role.Name == FlagAccess.ViewRole && x.Permission.Name == FlagAccess.ResponseFlagsView) ||
             (x.Role.Name == FlagAccess.CreateRole && x.Permission.Name == FlagAccess.ResponseFlagsCreate) ||
             (x.Role.Name == FlagAccess.AnswerRole && x.Permission.Name == FlagAccess.ResponseFlagsAnswer) ||
             (x.Role.Name == FlagAccess.EditBriefRole && x.Permission.Name == FlagAccess.ResponseFlagsEditBrief)));
        AddOrderBy(x => x.Role.Name);
        Select(x => new FlagCapabilityRole(x.RoleId, x.Role.Name, x.Permission.Name));
    }
}

internal sealed class CapabilityAssignmentsSpec : Specification<UserRole>
{
    public CapabilityAssignmentsSpec(Guid userId)
    {
        AddCriteria(x => x.ApplicationUserId == userId && x.Role.IsSystemRole &&
            (x.Role.Name == FlagAccess.ViewRole || x.Role.Name == FlagAccess.CreateRole ||
             x.Role.Name == FlagAccess.AnswerRole || x.Role.Name == FlagAccess.EditBriefRole));
    }
}
