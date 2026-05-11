using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.RestoreTemplate
{
    internal sealed class RestoreTemplateCommandValidator
         : AbstractValidator<RestoreTemplateCommand>
    {
        public RestoreTemplateCommandValidator()
        {
            RuleFor(x => x.TemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.RestoreTemplate_TemplateId_Required);
        }
    }
}