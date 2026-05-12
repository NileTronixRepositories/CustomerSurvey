using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    internal sealed record DepartmentAdminForAssignTemplatesToOperatorDto
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentDepartmentAdminForAssignTemplatesToOperatorSpec
        : Specification<DepartmentAdmin, DepartmentAdminForAssignTemplatesToOperatorDto>
    {
        public GetCurrentDepartmentAdminForAssignTemplatesToOperatorSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new DepartmentAdminForAssignTemplatesToOperatorDto
            {
                DepartmentAdminId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}