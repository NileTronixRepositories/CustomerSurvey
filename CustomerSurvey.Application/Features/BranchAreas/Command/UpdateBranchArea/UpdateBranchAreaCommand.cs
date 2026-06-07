using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.UpdateBranchArea
{
    public sealed record UpdateBranchAreaCommand : ICommand<UpdateBranchAreaResponse>
    {
        public Guid BranchAreaId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }
}
