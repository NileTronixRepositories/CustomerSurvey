using CustomerSurvey.Application.Features.BranchAreas.Shared;

namespace CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea
{
    public sealed record CreateBranchAreaResponse
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public IReadOnlyCollection<BranchAreaBranchItemResponse> Branches { get; init; } =
            Array.Empty<BranchAreaBranchItemResponse>();
    }
}
