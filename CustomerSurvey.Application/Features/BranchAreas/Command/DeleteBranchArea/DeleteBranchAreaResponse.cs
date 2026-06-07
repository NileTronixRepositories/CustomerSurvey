namespace CustomerSurvey.Application.Features.BranchAreas.Command.DeleteBranchArea
{
    public sealed record DeleteBranchAreaResponse
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public bool IsActive { get; init; }
    }
}
