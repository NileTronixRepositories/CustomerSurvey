using BuildingBlock.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    using DomainOperator = Domain.Identity.Operator;

    internal sealed record OperatorForAssignTemplatesToOperatorDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorForAssignTemplatesToOperatorSpec
        : Specification<DomainOperator, OperatorForAssignTemplatesToOperatorDto>
    {
        public GetOperatorForAssignTemplatesToOperatorSpec(Guid operatorId)
        {
            AddCriteria(x => x.Id == operatorId);

            Select(x => new OperatorForAssignTemplatesToOperatorDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}