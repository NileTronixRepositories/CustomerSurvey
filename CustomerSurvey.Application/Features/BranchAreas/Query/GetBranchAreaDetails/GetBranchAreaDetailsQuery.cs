using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreaDetails
{
    public sealed record GetBranchAreaDetailsQuery : IQuery<BranchAreaDetailsResponse>
    {
        public Guid BranchAreaId { get; init; }
    }
}
