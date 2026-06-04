using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed class GetBranchTemplatesReportQueryValidator
    : AbstractValidator<GetBranchTemplatesReportQuery>
{
    public GetBranchTemplatesReportQueryValidator()
    {
        BranchTemplatesReportQueryValidationRules.AddRules(this);
    }
}
