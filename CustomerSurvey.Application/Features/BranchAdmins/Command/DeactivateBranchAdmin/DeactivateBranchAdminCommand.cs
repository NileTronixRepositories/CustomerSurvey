using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin
{
    public sealed record DeactivateBranchAdminCommand
        : ICommand<DeactivateBranchAdminResponse>
    {
        public Guid BranchAdminId { get; init; }
    }
}
