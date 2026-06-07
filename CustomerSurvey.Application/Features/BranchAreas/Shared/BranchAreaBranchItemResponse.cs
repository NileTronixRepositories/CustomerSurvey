namespace CustomerSurvey.Application.Features.BranchAreas.Shared
{
    public sealed record BranchAreaBranchItemResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;
    }
}
