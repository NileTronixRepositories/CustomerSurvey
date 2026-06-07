using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea
{
    public sealed record AssignBranchesToBranchAreaCommand : ICommand<AssignBranchesToBranchAreaResponse>
    {
        public Guid BranchAreaId { get; init; }

        public IReadOnlyCollection<Guid> BranchIds { get; init; } = Array.Empty<Guid>();
    }
}
