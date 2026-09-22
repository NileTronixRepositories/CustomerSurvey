using BuildingBlock.Application.Abstraction;
using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplateLogo;

public sealed record UpdateTemplateLogoCommand : ICommand<UpdateTemplateLogoResponse>
{
    public Guid TemplateId { get; init; }
    public IFormFile Logo { get; init; } = null!;
}

public sealed record UpdateTemplateLogoResponse(Guid TemplateId, string LogoPath);
