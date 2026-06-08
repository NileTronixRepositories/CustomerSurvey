using BuildingBlock.Domain.Specification;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    internal sealed record OperatorForRestoreOperatorDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }
    }

    internal sealed class GetOperatorForRestoreOperatorSpec
        : Specification<DomainOperator, OperatorForRestoreOperatorDto>
    {
        public GetOperatorForRestoreOperatorSpec(Guid operatorId)
        {
            AddCriteria(x => x.Id == operatorId);

            Select(x => new OperatorForRestoreOperatorDto
            {
                OperatorId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                DepartmentId = x.DepartmentId
            });
        }
    }
}
