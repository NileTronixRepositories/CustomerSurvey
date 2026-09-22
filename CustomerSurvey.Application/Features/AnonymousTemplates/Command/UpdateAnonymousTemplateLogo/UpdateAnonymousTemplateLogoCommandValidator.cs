using CustomerSurvey.Application.Shared.Media;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplateLogo;

internal sealed class UpdateAnonymousTemplateLogoCommandValidator
    : AbstractValidator<UpdateAnonymousTemplateLogoCommand>
{
    public UpdateAnonymousTemplateLogoCommandValidator()
    {
        RuleFor(x => x.AnonymousTemplateId).NotEmpty();
        RuleFor(x => x.Logo).Custom((logo, context) =>
        {
            var error = TemplateLogoMedia.Validate(logo);
            if (error is not null)
            {
                context.AddFailure(nameof(UpdateAnonymousTemplateLogoCommand.Logo), error.Message);
            }
        });
    }
}
