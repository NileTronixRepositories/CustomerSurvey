namespace CustomerSurvey.Application.Features.Operators.Command.DeactivateOperator
{
    public sealed record DeactivateOperatorResponse
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }

        public bool IsActive { get; init; }
    }
}
