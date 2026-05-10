using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    internal sealed class GetDepartmentAdminsForDepartmentDetailsSpec
       : Specification<DepartmentAdmin, DepartmentDetailsDepartmentAdminResponse>
    {
        public GetDepartmentAdminsForDepartmentDetailsSpec(Guid departmentId)
        {
            AddCriteria(x => x.DepartmentId == departmentId);

            AddOrderBy(x => x.ApplicationUser.NameEn);

            Select(x => new DepartmentDetailsDepartmentAdminResponse
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                NameEn = x.ApplicationUser.NameEn,
                NameAr = x.ApplicationUser.NameAr,
                UserName = x.ApplicationUser.UserName,
                Email = x.ApplicationUser.Email ?? string.Empty,
                PhoneNumber = x.ApplicationUser.PhoneNumber
            });
        }
    }
}