using BuildingBlock.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed record OperatorForTemplatesSelectionDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorForTemplatesSelectionSpec
        : Specification<DomainOperator, OperatorForTemplatesSelectionDto>
    {
        public GetOperatorForTemplatesSelectionSpec(Guid operatorId)
        {
            AddCriteria(x => x.Id == operatorId);

            Select(x => new OperatorForTemplatesSelectionDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}