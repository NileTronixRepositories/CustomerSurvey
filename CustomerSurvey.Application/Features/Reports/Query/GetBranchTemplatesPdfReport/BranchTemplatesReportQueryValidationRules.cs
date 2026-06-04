using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal static class BranchTemplatesReportQueryValidationRules
{
    public static void AddRules<TQuery>(AbstractValidator<TQuery> validator)
        where TQuery : IBranchTemplatesReportQueryParameters
    {
        validator.RuleFor(x => x.FromDate)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_FromDate_Required);

        validator.RuleFor(x => x.ToDate)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ToDate_Required);

        validator.RuleFor(x => x)
            .Must(x => x.FromDate <= x.ToDate)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_DateRange_Invalid);

        validator.RuleFor(x => x)
            .Must(x => GetInclusiveDays(x.FromDate, x.ToDate) >= 1)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_MinDuration_Invalid);

        validator.RuleFor(x => x)
            .Must(x => x.ToDate <= x.FromDate.AddMonths(12).AddDays(-1))
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_MaxDuration_Invalid);

        validator.RuleFor(x => x.Language)
            .Must(x =>
                string.Equals(x, "ar", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x, "en", StringComparison.OrdinalIgnoreCase))
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_Language_Invalid);

        validator.RuleFor(x => x.ScoreCalculationMode)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ScoreCalculationMode_Invalid);

        validator.RuleFor(x => x.TemplateKind)
            .IsInEnum()
            .When(x => x.TemplateKind.HasValue)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_TemplateKind_Invalid);

        validator.RuleFor(x => x.TopWorstQuestionsCount)
            .Must(x => x is 5 or 10 or 20)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_TopWorstQuestionsCount_Invalid);
    }

    private static int GetInclusiveDays(DateOnly fromDate, DateOnly toDate)
        => toDate.DayNumber - fromDate.DayNumber + 1;
}
