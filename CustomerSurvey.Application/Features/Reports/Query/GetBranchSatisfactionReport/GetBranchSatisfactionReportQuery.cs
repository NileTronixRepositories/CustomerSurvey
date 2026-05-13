using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    public sealed record GetBranchSatisfactionReportQuery
         : IQuery<GetBranchSatisfactionReportResponse>
    {
        public DateOnly? From { get; init; }

        public DateOnly? To { get; init; }

        public Guid? TemplateId { get; init; }
    }
}