namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    public sealed record TemplatePaginationItemResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public string Status { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public int QuestionsCount { get; init; }

        public TemplatePaginationCreatedByResponse? CreatedBy { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }
    }

    public sealed record TemplatePaginationCreatedByResponse
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}