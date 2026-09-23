using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;

internal sealed class GetDepartmentSurveyResponsesPaginationQueryValidator
    : AbstractValidator<GetDepartmentSurveyResponsesPaginationQuery>
{
    public GetDepartmentSurveyResponsesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x).Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From must be on or before To.");
        RuleFor(x => x.MinScorePercentage).InclusiveBetween(0m, 100m).When(x => x.MinScorePercentage.HasValue);
        RuleFor(x => x.MaxScorePercentage).InclusiveBetween(0m, 100m).When(x => x.MaxScorePercentage.HasValue);
        RuleFor(x => x).Must(x =>
                !x.MinScorePercentage.HasValue ||
                !x.MaxScorePercentage.HasValue ||
                x.MinScorePercentage <= x.MaxScorePercentage)
            .WithMessage("MinScorePercentage must be less than or equal to MaxScorePercentage.");
        RuleFor(x => x.CustomInputType).NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputType is required when CustomInputValue is supplied.");
        RuleFor(x => x.CustomInputValue).Must(value => int.TryParse(value, out _))
            .When(x =>
                x.CustomInputType == CustomerSurvey.Domain.Enums.TemplateCustomInputType.Integer &&
                !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputValue must be a valid integer for an Integer custom input.");
        RuleFor(x => x.SatisfactionCategory).IsInEnum()
            .When(x => x.SatisfactionCategory.HasValue)
            .WithMessage("SatisfactionCategory is invalid.");
    }
}
