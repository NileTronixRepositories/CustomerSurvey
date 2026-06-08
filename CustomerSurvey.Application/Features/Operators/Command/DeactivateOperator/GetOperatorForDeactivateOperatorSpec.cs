using BuildingBlock.Domain.Specification;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    internal sealed record OperatorForDeactivateOperatorDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorForDeactivateOperatorSpec
        : Specification<DomainOperator, OperatorForDeactivateOperatorDto>
    {
        public GetOperatorForDeactivateOperatorSpec(Guid operatorId)
        {
            AddCriteria(x => x.Id == operatorId);

            Select(x => new OperatorForDeactivateOperatorDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}
