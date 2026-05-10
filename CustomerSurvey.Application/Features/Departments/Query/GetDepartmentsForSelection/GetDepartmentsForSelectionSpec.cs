using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsForSelection
{
    internal sealed class GetDepartmentsForSelectionSpec
        : Specification<Department, DepartmentSelectionResponse>
    {
        public GetDepartmentsForSelectionSpec()
        {
            AddCriteria(x => x.IsActive);

            AddOrderBy(x => x.NameEn);

            Select(x => new DepartmentSelectionResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}