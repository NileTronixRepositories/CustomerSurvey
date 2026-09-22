using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;

public sealed record CopyTemplateAsAnonymousCommand : ICommand<CopyTemplateAsAnonymousResponse>
{
    public Guid TemplateId { get; init; }
}

public sealed record CopyTemplateAsAnonymousResponse
{
    public Guid SourceTemplateId { get; init; }
    public Guid AnonymousTemplateId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public Guid TemplateFamilyId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? LogoPath { get; init; }
    public string? PublicUrl { get; init; }
    public string? QrCode { get; init; }
    public int QuestionsCount { get; init; }
    public int ConditionsCount { get; init; }
}
