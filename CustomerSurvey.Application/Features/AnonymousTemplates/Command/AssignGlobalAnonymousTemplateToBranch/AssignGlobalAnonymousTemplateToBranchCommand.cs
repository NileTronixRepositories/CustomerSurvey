using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;

public sealed record AssignGlobalAnonymousTemplateToBranchCommand
    : ICommand<AssignGlobalAnonymousTemplateToBranchResponse>
{
    public Guid GlobalTemplateId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime ActiveFrom { get; init; }
    public DateTime? ExpireTo { get; init; }
    public IFormFile? Logo { get; init; }
}

public sealed record AssignGlobalAnonymousTemplateToBranchResponse
{
    public Guid AnonymousTemplateId { get; init; }
    public Guid SourceGlobalAnonymousTemplateId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchNameEn { get; init; } = string.Empty;
    public string? BranchNameAr { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public DateTime ActiveFrom { get; init; }
    public DateTime? ExpireTo { get; init; }
    public bool IsActive { get; init; }
    public string? PublicUrl { get; init; }
    public string? QrCode { get; init; }
    public string? LogoPath { get; init; }
    public int QuestionsCount { get; init; }
    public int ConditionsCount { get; init; }
    public int CustomInputsCount { get; init; }
}
