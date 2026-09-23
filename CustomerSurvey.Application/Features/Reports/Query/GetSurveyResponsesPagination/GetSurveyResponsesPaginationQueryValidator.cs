using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;

internal sealed class GetSurveyResponsesPaginationQueryValidator
    : AbstractValidator<GetSurveyResponsesPaginationQuery>
{
    public GetSurveyResponsesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value.Date <= x.To.Value.Date)
            .WithMessage("From must be on or before To.");
        RuleFor(x => x).Must(x => !x.TemplateId.HasValue || !x.AnonymousTemplateId.HasValue)
            .WithMessage("TemplateId and AnonymousTemplateId cannot both be supplied.");
        RuleFor(x => x.CustomInputType).NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputType is required when CustomInputValue is supplied.");
        RuleFor(x => x.CustomInputValue).Must(value => int.TryParse(value, out _))
            .When(x =>
                x.CustomInputType == CustomerSurvey.Domain.Enums.TemplateCustomInputType.Integer &&
                !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputValue must be a valid integer for an Integer custom input.");
        RuleFor(x => x.Source).IsInEnum();
        RuleFor(x => x.ScoreCalculationMode).IsInEnum();
        RuleFor(x => x.SatisfactionCategory).IsInEnum()
            .When(x => x.SatisfactionCategory.HasValue)
            .WithMessage("SatisfactionCategory is invalid.");
        RuleFor(x => x).Must(x =>
                !(x.CustomInputLabelEnIsNull == true && !string.IsNullOrWhiteSpace(x.CustomInputLabelEn)) &&
                !(x.CustomInputLabelArIsNull == true && !string.IsNullOrWhiteSpace(x.CustomInputLabelAr)))
            .WithMessage("A custom-input label cannot have both a value and an is-null filter.");
    }
}
