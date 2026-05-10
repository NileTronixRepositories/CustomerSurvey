using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin
{
    internal sealed class GetDepartmentForCreateDepartmentAdminSpec
        : Specification<Department>
    {
        public GetDepartmentForCreateDepartmentAdminSpec(Guid departmentId)
        {
            AddCriteria(x => x.Id == departmentId);
        }
    }
}