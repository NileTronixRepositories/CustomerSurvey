using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    internal sealed record DepartmentAdminForOperatorsPaginationDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForOperatorsPaginationSpec
        : Specification<DepartmentAdmin, DepartmentAdminForOperatorsPaginationDto>
    {
        public GetCurrentDepartmentAdminForOperatorsPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForOperatorsPaginationDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}