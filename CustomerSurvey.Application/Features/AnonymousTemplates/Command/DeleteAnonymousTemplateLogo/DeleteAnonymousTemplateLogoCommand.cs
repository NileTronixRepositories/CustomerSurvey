using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplateLogo;

public sealed record DeleteAnonymousTemplateLogoCommand : ICommand<DeleteAnonymousTemplateLogoResponse>
{
    public Guid AnonymousTemplateId { get; init; }
}

public sealed record DeleteAnonymousTemplateLogoResponse(Guid AnonymousTemplateId, string? LogoPath);
