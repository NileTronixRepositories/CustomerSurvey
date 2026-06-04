using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    public sealed record SelectBranchCommand : ICommand<SelectBranchResponse>
    {
        public Guid BranchId { get; init; }
    }
}
