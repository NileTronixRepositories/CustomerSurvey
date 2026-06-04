using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.RestoreBranchArea
{
    public sealed record RestoreBranchAreaCommand : ICommand<RestoreBranchAreaResponse>
    {
        public Guid BranchAreaId { get; init; }
    }
}
