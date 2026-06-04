namespace CustomerSurvey.Application.Features.BranchAreas.Command.UpdateBranchArea
{
    public sealed record UpdateBranchAreaResponse
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }
    }
}
