using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;

internal sealed class GetSystemSurveyResponsesPaginationQueryValidator
    : AbstractValidator<GetSystemSurveyResponsesPaginationQuery>
{
    public GetSystemSurveyResponsesPaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_PageNumber_Invalid);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_PageSize_Invalid);

        RuleFor(x => x)
            .Must(x =>
                !x.From.HasValue ||
                !x.To.HasValue ||
                x.From.Value <= x.To.Value)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_DateRange_Invalid);

        RuleFor(x => x.MinScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MinScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_MinScore_Invalid);

        RuleFor(x => x.MaxScorePercentage)
            .InclusiveBetween(0m, 100m)
            .When(x => x.MaxScorePercentage.HasValue)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_MaxScore_Invalid);

        RuleFor(x => x)
            .Must(x =>
                !x.MinScorePercentage.HasValue ||
                !x.MaxScorePercentage.HasValue ||
                x.MinScorePercentage.Value <= x.MaxScorePercentage.Value)
            .WithMessage(ErrorMessage.GetSystemSurveyResponsesPagination_ScoreRange_Invalid);

        RuleFor(x => x.CustomInputType)
            .NotNull()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputType is required when CustomInputValue is supplied.");

        RuleFor(x => x.CustomInputValue)
            .Must(value => int.TryParse(value, out _))
            .When(x =>
                x.CustomInputType == CustomerSurvey.Domain.Enums.TemplateCustomInputType.Integer &&
                !string.IsNullOrWhiteSpace(x.CustomInputValue))
            .WithMessage("CustomInputValue must be a valid integer for an Integer custom input.");

        RuleFor(x => x.SatisfactionCategory)
            .IsInEnum()
            .When(x => x.SatisfactionCategory.HasValue)
            .WithMessage("SatisfactionCategory is invalid.");
    }
}
