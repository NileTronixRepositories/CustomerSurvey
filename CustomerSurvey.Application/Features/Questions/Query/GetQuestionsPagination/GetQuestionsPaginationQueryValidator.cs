using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetQuestionsPaginationQueryValidator
        : AbstractValidator<GetQuestionsPaginationQuery>
    {
        public GetQuestionsPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetQuestionsPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetQuestionsPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetQuestionsPagination_PageSize_Max);
        }
    }
}