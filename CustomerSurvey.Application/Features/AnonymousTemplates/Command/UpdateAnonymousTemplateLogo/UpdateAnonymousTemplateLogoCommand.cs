using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplateLogo;

public sealed record UpdateAnonymousTemplateLogoCommand : ICommand<UpdateAnonymousTemplateLogoResponse>
{
    public Guid AnonymousTemplateId { get; init; }
    public IFormFile Logo { get; init; } = null!;
}

public sealed record UpdateAnonymousTemplateLogoResponse(Guid AnonymousTemplateId, string LogoPath);
