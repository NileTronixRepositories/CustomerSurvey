using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Api.Contracts.AnonymousTemplates;

public sealed record UpdateAnonymousTemplateLogoRequest
{
    public IFormFile Logo { get; init; } = null!;
}
