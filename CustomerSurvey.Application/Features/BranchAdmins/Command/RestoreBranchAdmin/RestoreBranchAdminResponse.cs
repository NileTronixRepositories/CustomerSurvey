namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    public sealed record RestoreBranchAdminResponse
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public bool IsActive { get; init; }
    }
}
