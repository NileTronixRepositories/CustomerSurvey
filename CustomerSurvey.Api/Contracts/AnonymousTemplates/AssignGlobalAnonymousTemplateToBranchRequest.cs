using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Api.Contracts.AnonymousTemplates;

public sealed record AssignGlobalAnonymousTemplateToBranchRequest
{
    public Guid BranchId { get; init; }
    public DateTime ActiveFrom { get; init; }
    public DateTime? ExpireTo { get; init; }
    public IFormFile? Logo { get; init; }
}
