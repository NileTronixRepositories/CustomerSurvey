namespace CustomerSurvey.Api.Contracts.BranchAreas
{
    public sealed class AssignBranchesToBranchAreaRequest
    {
        public IReadOnlyCollection<Guid> BranchIds { get; init; } = Array.Empty<Guid>();
    }
}
