using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser
{
    internal sealed record RoleForAssignRolesToBranchUserDto
    {
        public Guid RoleId { get; init; }

        public string Name { get; init; } = string.Empty;
    }

    internal sealed class GetRolesForAssignRolesToBranchUserSpec
        : Specification<Role, RoleForAssignRolesToBranchUserDto>
    {
        public GetRolesForAssignRolesToBranchUserSpec(IReadOnlyCollection<Guid> roleIds)
        {
            var ids = roleIds
                .Distinct()
                .ToArray();

            AddCriteria(x => ids.Contains(x.Id));

            Select(x => new RoleForAssignRolesToBranchUserDto
            {
                RoleId = x.Id,
                Name = x.Name
            });
        }
    }
}