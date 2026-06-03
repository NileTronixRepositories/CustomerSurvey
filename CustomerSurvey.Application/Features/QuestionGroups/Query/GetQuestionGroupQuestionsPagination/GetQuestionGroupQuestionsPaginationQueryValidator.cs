using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetQuestionGroupQuestionsPaginationQueryValidator
        : AbstractValidator<GetQuestionGroupQuestionsPaginationQuery>
    {
        public GetQuestionGroupQuestionsPaginationQueryValidator()
        {
            RuleFor(x => x.QuestionGroupId)
                .NotEmpty()
                .WithMessage(ErrorMessage.CreateQuestion_GroupId_Required);

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
