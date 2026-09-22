using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplateLogo;

public sealed record DeleteTemplateLogoCommand : ICommand<DeleteTemplateLogoResponse>
{
    public Guid TemplateId { get; init; }
}

public sealed record DeleteTemplateLogoResponse(Guid TemplateId, string? LogoPath);
