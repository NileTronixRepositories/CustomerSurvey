using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

public sealed record SuperAdminTemplatePaginationItemResponse
{
    public Guid TemplateId { get; init; }

    public Guid? BranchId { get; init; }

    public string? BranchNameEn { get; init; }

    public string? BranchNameAr { get; init; }

    public TemplateCatalogKind TemplateKind { get; init; }

    public string TemplateKindName => TemplateKind.ToString();

    public AnonymousTemplateScope? Scope { get; init; }

    public string? ScopeName => Scope?.ToString();

    public bool IsGlobal { get; init; }

    public bool IsArchived { get; init; }

    public Guid? SourceGlobalAnonymousTemplateId { get; init; }

    public bool IsManagedGlobalCopy { get; init; }

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

    public SuperAdminTemplatePaginationCreatedByResponse? CreatedBy { get; init; }

    public DateTime CreatedOnUtc { get; init; }

    public DateTime ActiveFrom { get; init; }

    public DateTime? ExpireTo { get; init; }
}

public sealed record SuperAdminTemplatePaginationCreatedByResponse
{
    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}
