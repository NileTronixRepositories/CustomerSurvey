namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment
{
    public sealed record RestoreDepartmentResponse
    {
        public Guid DepartmentId { get; init; }

        public bool IsActive { get; init; }
    }
}
