using BuildingBlock.Domain.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    using DomainOperator = Domain.Identity.Operator;

    internal sealed record CurrentOperatorForMyTemplatesDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetCurrentOperatorForMyTemplatesSpec
        : Specification<DomainOperator, CurrentOperatorForMyTemplatesDto>
    {
        public GetCurrentOperatorForMyTemplatesSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentOperatorForMyTemplatesDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}