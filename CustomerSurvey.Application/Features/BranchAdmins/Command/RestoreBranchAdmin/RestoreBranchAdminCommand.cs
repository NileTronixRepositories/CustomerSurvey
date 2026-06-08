using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    public sealed record RestoreBranchAdminCommand
        : ICommand<RestoreBranchAdminResponse>
    {
        public Guid BranchAdminId { get; init; }
    }
}
