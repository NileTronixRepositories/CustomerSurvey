using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;

internal sealed class GetSurveyDashboardTemplatesSelectionQueryValidator
    : AbstractValidator<GetSurveyDashboardTemplatesSelectionQuery>
{
    public GetSurveyDashboardTemplatesSelectionQueryValidator()
    {
        RuleFor(x => x.TemplateKind)
            .IsInEnum()
            .When(x => x.TemplateKind.HasValue)
            .WithMessage(ErrorMessage.SurveyDashboardTemplatesSelection_TemplateKind_Invalid);
    }
}
