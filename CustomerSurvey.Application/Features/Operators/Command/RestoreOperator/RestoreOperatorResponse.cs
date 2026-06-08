namespace CustomerSurvey.Application.Features.Operators.Command.RestoreOperator
{
    public sealed record RestoreOperatorResponse
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }

        public bool IsActive { get; init; }
    }
}
