using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination
{
    internal sealed class GetGlobalQuestionsPaginationQueryValidator
        : AbstractValidator<GetGlobalQuestionsPaginationQuery>
    {
        public GetGlobalQuestionsPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetGlobalQuestionsPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetGlobalQuestionsPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetGlobalQuestionsPagination_PageSize_Max);
        }
    }
}