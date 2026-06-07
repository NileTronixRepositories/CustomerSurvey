using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea
{
    public sealed record CreateBranchAreaCommand : ICommand<CreateBranchAreaResponse>
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string Password { get; init; } = string.Empty;

        public IReadOnlyCollection<Guid> BranchIds { get; init; } = Array.Empty<Guid>();
    }
}
