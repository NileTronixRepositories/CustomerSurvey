namespace CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin
{
    public sealed record DeactivateBranchAdminResponse
    {
        public Guid BranchAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public bool IsActive { get; init; }
    }
}
