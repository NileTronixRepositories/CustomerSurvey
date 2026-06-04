using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.DeleteBranchArea
{
    public sealed record DeleteBranchAreaCommand : ICommand<DeleteBranchAreaResponse>
    {
        public Guid BranchAreaId { get; init; }
    }
}
