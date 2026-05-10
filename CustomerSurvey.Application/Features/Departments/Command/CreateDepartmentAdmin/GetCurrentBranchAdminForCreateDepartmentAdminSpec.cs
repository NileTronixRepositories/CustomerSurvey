using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin
{
    internal sealed class GetCurrentBranchAdminForCreateDepartmentAdminSpec
           : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForCreateDepartmentAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}