using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    public sealed record DeactivateOperatorCommand
        : ICommand<DeactivateOperatorResponse>
    {
        public Guid OperatorId { get; init; }
    }
}
