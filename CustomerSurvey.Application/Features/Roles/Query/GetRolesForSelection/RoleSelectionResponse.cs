using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Roles.Query.GetRolesForSelection
{
    public sealed record RoleSelectionResponse
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}