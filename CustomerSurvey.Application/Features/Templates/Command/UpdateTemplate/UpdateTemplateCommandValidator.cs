using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed class UpdateTemplateCommandValidator
          : AbstractValidator<UpdateTemplateCommand>
    {
        public UpdateTemplateCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateTemplate_TemplateId_Required);

            RuleFor(x => x.NameEn)
                .NotEmpty()
                .WithMessage(ErrorMessage.UpdateTemplate_NameEn_Required)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateTemplate_NameEn_MaxLength);

            RuleFor(x => x.NameAr)
                .MaximumLength(200)
                .WithMessage(ErrorMessage.UpdateTemplate_NameAr_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.NameAr));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage(ErrorMessage.UpdateTemplate_Description_MaxLength)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}