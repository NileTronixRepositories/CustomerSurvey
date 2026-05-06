using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed record UserPermissionForLoginDto
    {
        public string PermissionName { get; init; } = string.Empty;
    }

    internal sealed class GetUserPermissionsForLoginSpec
        : Specification<RolePermission, UserPermissionForLoginDto>
    {
        public GetUserPermissionsForLoginSpec(IReadOnlyCollection<Guid> roleIds)
        {
            AddCriteria(x => roleIds.Contains(x.RoleId));

            Select(x => new UserPermissionForLoginDto
            {
                PermissionName = x.Permission.Name
            });
        }
    }
}