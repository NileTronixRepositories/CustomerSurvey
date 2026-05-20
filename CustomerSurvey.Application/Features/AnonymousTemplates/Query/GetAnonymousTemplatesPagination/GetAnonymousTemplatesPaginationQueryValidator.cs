using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    internal sealed class GetAnonymousTemplatesPaginationQueryValidator
         : AbstractValidator<GetAnonymousTemplatesPaginationQuery>
    {
        public GetAnonymousTemplatesPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetAnonymousTemplatesPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetAnonymousTemplatesPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetAnonymousTemplatesPagination_PageSize_Max);

            RuleFor(x => x.Scope)
                .Must(x => !x.HasValue ||
                           x.Value == AnonymousTemplateScope.Branch ||
                           x.Value == AnonymousTemplateScope.Global)
                .WithMessage(ErrorMessage.GetAnonymousTemplatesPagination_Scope_Invalid);
        }
    }
}