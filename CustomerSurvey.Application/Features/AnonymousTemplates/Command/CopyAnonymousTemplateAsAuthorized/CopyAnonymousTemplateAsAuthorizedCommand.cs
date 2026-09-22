using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;

public sealed record CopyAnonymousTemplateAsAuthorizedCommand
    : ICommand<CopyAnonymousTemplateAsAuthorizedResponse>
{
    public Guid AnonymousTemplateId { get; init; }
}

public sealed record CopyAnonymousTemplateAsAuthorizedResponse
{
    public Guid SourceAnonymousTemplateId { get; init; }
    public Guid TemplateId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public Guid TemplateFamilyId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? LogoPath { get; init; }
    public int QuestionsCount { get; init; }
    public int ConditionsCount { get; init; }
}
