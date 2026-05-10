using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    internal sealed class GetOperatorsForDepartmentDetailsSpec
        : Specification<Operator, DepartmentDetailsOperatorResponse>
    {
        public GetOperatorsForDepartmentDetailsSpec(Guid departmentId)
        {
            AddCriteria(x => x.DepartmentId == departmentId);

            AddOrderBy(x => x.ApplicationUser.NameEn);

            Select(x => new DepartmentDetailsOperatorResponse
            {
                OperatorId = x.Id,
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