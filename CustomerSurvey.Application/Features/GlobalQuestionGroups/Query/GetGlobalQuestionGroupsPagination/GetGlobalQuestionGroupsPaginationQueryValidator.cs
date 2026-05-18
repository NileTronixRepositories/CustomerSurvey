using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    internal sealed class GetGlobalQuestionGroupsPaginationQueryValidator
         : AbstractValidator<GetGlobalQuestionGroupsPaginationQuery>
    {
        public GetGlobalQuestionGroupsPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetGlobalQuestionGroupsPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetGlobalQuestionGroupsPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetGlobalQuestionGroupsPagination_PageSize_Max);
        }
    }
}