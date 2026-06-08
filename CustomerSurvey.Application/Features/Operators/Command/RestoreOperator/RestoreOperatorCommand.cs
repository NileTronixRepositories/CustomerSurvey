using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    public sealed record RestoreOperatorCommand
        : ICommand<RestoreOperatorResponse>
    {
        public Guid OperatorId { get; init; }
    }
}
