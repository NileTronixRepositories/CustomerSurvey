using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.UpdateOperator
{
    internal sealed record DepartmentAdminForUpdateOperatorDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForUpdateOperatorSpec
        : Specification<DepartmentAdmin, DepartmentAdminForUpdateOperatorDto>
    {
        public GetCurrentDepartmentAdminForUpdateOperatorSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForUpdateOperatorDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}