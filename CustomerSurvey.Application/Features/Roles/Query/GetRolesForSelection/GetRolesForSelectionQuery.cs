using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Roles.Query.GetRolesForSelection
{
    public sealed record GetRolesForSelectionQuery
       : IQuery<IReadOnlyCollection<RoleSelectionResponse>>;
}