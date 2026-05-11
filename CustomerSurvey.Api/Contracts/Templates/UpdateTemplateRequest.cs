namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class UpdateTemplateRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }
    }
}