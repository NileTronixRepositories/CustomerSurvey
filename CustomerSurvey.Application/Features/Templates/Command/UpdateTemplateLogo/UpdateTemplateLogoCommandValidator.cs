using CustomerSurvey.Application.Shared.Media;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplateLogo;

internal sealed class UpdateTemplateLogoCommandValidator : AbstractValidator<UpdateTemplateLogoCommand>
{
    public UpdateTemplateLogoCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Logo).Custom((logo, context) =>
        {
            var error = TemplateLogoMedia.Validate(logo);
            if (error is not null)
            {
                context.AddFailure(nameof(UpdateTemplateLogoCommand.Logo), error.Message);
            }
        });
    }
}
