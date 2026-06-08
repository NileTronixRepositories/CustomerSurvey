using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin
{
    public sealed record DeactivateDepartmentAdminCommand
        : ICommand<DeactivateDepartmentAdminResponse>
    {
        public Guid DepartmentAdminId { get; init; }
    }
}
