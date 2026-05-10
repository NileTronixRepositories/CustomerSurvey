using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartment
{
    internal sealed class GetCurrentBranchAdminForCreateDepartmentSpec
         : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForCreateDepartmentSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}