using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

public sealed record CopyTemplateToBranchResponse
{
    public Guid SourceTemplateId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public TemplateCatalogKind TemplateKind { get; init; }

    public string TemplateKindName => TemplateKind.ToString();

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string? Description { get; init; }

    public DateTime ActiveFrom { get; init; }

    public DateTime? ExpireTo { get; init; }

    public bool IsActive { get; init; }

    public string? LogoPath { get; init; }

    public int QuestionsCount { get; init; }

    public int ConditionsCount { get; init; }

    public int CustomInputsCount { get; init; }

    public string? PublicUrl { get; init; }

    public string? QrCode { get; init; }
}
