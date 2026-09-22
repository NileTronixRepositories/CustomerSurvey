using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed record SuperAdminTemplatePaginationItemDto
{
    public Guid TemplateId { get; init; }

    public Guid BranchId { get; init; }

    public string? BranchNameEn { get; init; }

    public string? BranchNameAr { get; init; }

    public TemplateCatalogKind TemplateKind { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public string? LogoPath { get; init; }

    public int QuestionsCount { get; init; }

    public int CustomInputsCount { get; init; }

    public string? PublicUrl { get; init; }

    public string? QrCode { get; init; }

    public Guid CreatedByApplicationUserId { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime ActiveFrom { get; init; }

    public DateTime? ExpireTo { get; init; }
}
