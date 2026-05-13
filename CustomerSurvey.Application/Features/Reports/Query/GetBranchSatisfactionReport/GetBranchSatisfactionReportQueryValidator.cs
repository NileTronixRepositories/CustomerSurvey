using CustomerSurvey.Domain.Resources;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetBranchSatisfactionReportQueryValidator
          : AbstractValidator<GetBranchSatisfactionReportQuery>
    {
        public GetBranchSatisfactionReportQueryValidator()
        {
            RuleFor(x => x)
                .Must(x =>
                    !x.From.HasValue ||
                    !x.To.HasValue ||
                    x.From.Value <= x.To.Value)
                .WithMessage(ErrorMessage.GetBranchSatisfactionReport_DateRange_Invalid);
        }
    }
}