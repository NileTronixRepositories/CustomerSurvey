using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed class GetBranchTemplatesPdfReportQueryValidator
    : AbstractValidator<GetBranchTemplatesPdfReportQuery>
{
    public GetBranchTemplatesPdfReportQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_FromDate_Required);

        RuleFor(x => x.ToDate)
            .NotEmpty()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ToDate_Required);

        RuleFor(x => x)
            .Must(x => x.FromDate <= x.ToDate)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_DateRange_Invalid);

        RuleFor(x => x)
            .Must(x => GetInclusiveDays(x.FromDate, x.ToDate) >= 1)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_MinDuration_Invalid);

        RuleFor(x => x)
            .Must(x => x.ToDate <= x.FromDate.AddMonths(12).AddDays(-1))
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_MaxDuration_Invalid);

        RuleFor(x => x.Language)
            .Must(x =>
                string.Equals(x, "ar", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x, "en", StringComparison.OrdinalIgnoreCase))
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_Language_Invalid);

        RuleFor(x => x.ScoreCalculationMode)
            .IsInEnum()
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_ScoreCalculationMode_Invalid);

        RuleFor(x => x.TemplateKind)
            .IsInEnum()
            .When(x => x.TemplateKind.HasValue)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_TemplateKind_Invalid);

        RuleFor(x => x.TopWorstQuestionsCount)
            .Must(x => x is 5 or 10 or 20)
            .WithMessage(ErrorMessage.GetBranchTemplatesPdfReport_TopWorstQuestionsCount_Invalid);
    }

    private static int GetInclusiveDays(DateOnly fromDate, DateOnly toDate)
        => toDate.DayNumber - fromDate.DayNumber + 1;
}
