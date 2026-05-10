namespace CustomerSurvey.Api.Contracts.Branches
{
    public sealed class AssignRolesToBranchUserRequest
    {
        public IReadOnlyCollection<Guid> RoleIds { get; init; } = Array.Empty<Guid>();
    }
}