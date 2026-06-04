using CustomerSurvey.Application.Features.BranchAreas.Shared;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea
{
    public sealed record AssignBranchesToBranchAreaResponse
    {
        public Guid BranchAreaId { get; init; }

        public IReadOnlyCollection<BranchAreaBranchItemResponse> Branches { get; init; } =
            Array.Empty<BranchAreaBranchItemResponse>();
    }
}
