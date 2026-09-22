using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed record AnonymousTemplateDashboardItemDto
{
    public Guid AnonymousTemplateId { get; init; }

    public Guid? BranchId { get; init; }

    public string? BranchNameEn { get; init; }

    public string? BranchNameAr { get; init; }

    public AnonymousTemplateScope Scope { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public bool IsActive { get; init; }

    public string? LogoPath { get; init; }

    public string? PublicUrl { get; init; }

    public string? QrCode { get; init; }
}
