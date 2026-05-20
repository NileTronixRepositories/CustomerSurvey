using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateResponsesPaginationQueryValidator
        : AbstractValidator<GetAnonymousTemplateResponsesPaginationQuery>
    {
        public GetAnonymousTemplateResponsesPaginationQueryValidator()
        {
            RuleFor(x => x.AnonymousTemplateId)
                .NotEmpty()
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_AnonymousTemplateId_Required);

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_PageNumber_Invalid);

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_PageSize_Invalid)
                .LessThanOrEqualTo(100)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_PageSize_Max);

            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue ||
                           !x.ToDate.HasValue ||
                           x.ToDate.Value > x.FromDate.Value)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_DateRange_Invalid);

            RuleFor(x => x.MinScorePercentage)
                .InclusiveBetween(0, 100)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_MinScorePercentage_Invalid)
                .When(x => x.MinScorePercentage.HasValue);

            RuleFor(x => x.MaxScorePercentage)
                .InclusiveBetween(0, 100)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_MaxScorePercentage_Invalid)
                .When(x => x.MaxScorePercentage.HasValue);

            RuleFor(x => x)
                .Must(x => !x.MinScorePercentage.HasValue ||
                           !x.MaxScorePercentage.HasValue ||
                           x.MaxScorePercentage.Value >= x.MinScorePercentage.Value)
                .WithMessage(ErrorMessage.GetAnonymousTemplateResponsesPagination_ScoreRange_Invalid);
        }
    }
}