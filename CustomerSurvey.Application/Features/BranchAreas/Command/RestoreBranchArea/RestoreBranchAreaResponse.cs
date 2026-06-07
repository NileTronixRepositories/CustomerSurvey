namespace CustomerSurvey.Application.Features.BranchAreas.Command.RestoreBranchArea
{
    public sealed record RestoreBranchAreaResponse
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public bool IsActive { get; init; }
    }
}
