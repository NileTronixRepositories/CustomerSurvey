using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails
{
    internal sealed class GetGlobalQuestionDetailsQueryValidator
        : AbstractValidator<GetGlobalQuestionDetailsQuery>
    {
        public GetGlobalQuestionDetailsQueryValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetGlobalQuestionDetails_QuestionId_Required);
        }
    }
}