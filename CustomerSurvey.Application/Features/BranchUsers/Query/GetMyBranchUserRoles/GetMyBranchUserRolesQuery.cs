using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles
{
    public sealed record GetMyBranchUserRolesQuery
       : IQuery<GetMyBranchUserRolesResponse>;
}