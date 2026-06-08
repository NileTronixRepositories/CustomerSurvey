using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment
{
    public sealed record RestoreDepartmentCommand : ICommand<RestoreDepartmentResponse>
    {
        public Guid DepartmentId { get; init; }
    }
}
