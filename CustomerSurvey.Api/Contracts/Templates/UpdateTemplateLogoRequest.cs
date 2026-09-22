using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Api.Contracts.Templates;

public sealed record UpdateTemplateLogoRequest
{
    public IFormFile Logo { get; init; } = null!;
}
