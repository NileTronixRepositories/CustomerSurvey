using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin
{
    public sealed record RestoreDepartmentAdminCommand
        : ICommand<RestoreDepartmentAdminResponse>
    {
        public Guid DepartmentAdminId { get; init; }
    }
}
