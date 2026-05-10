using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Roles.Query.GetRolesForSelection
{
    internal sealed class GetRolesForSelectionSpec
        : Specification<Role, RoleSelectionResponse>
    {
        public GetRolesForSelectionSpec(IReadOnlyCollection<string> allowedRoleNames)
        {
            var names = allowedRoleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray();

            AddCriteria(x => names.Contains(x.Name));

            AddOrderBy(x => x.Name);

            Select(x => new RoleSelectionResponse
            {
                Id = x.Id,
                Name = x.Name
            });
        }
    }
}