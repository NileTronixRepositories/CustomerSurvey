using BuildingBlock.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.UpdateOperator
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed record OperatorForUpdateOperatorDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorForUpdateOperatorSpec
        : Specification<DomainOperator, OperatorForUpdateOperatorDto>
    {
        public GetOperatorForUpdateOperatorSpec(Guid operatorId)
        {
            AddCriteria(x => x.Id == operatorId);

            Select(x => new OperatorForUpdateOperatorDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}