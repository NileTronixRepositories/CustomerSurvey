using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Resources;
using FluentValidation;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;

internal sealed class GetBranchTemplateExcelReportQueryValidator
    : AbstractValidator<GetBranchTemplateExcelReportQuery>
{
    public GetBranchTemplateExcelReportQueryValidator()
    {
        BranchTemplatesReportQueryValidationRules.AddRules(this);

        RuleFor(x => x.TemplateId)
            .NotNull()
            .Must(x => x.HasValue && x.Value != Guid.Empty)
            .WithMessage(ErrorMessage.GetTemplateDetails_TemplateId_Required);
    }
}
