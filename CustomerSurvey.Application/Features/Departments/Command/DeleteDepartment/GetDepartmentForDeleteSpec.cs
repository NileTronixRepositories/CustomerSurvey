using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment
{
    internal sealed class GetDepartmentForDeleteSpec
         : Specification<Department>
    {
        public GetDepartmentForDeleteSpec(Guid departmentId)
        {
            AddCriteria(x => x.Id == departmentId);
        }
    }
}