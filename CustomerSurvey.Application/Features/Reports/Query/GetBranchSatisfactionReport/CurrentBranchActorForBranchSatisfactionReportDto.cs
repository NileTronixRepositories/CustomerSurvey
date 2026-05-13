using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed record CurrentBranchActorForBranchSatisfactionReportDto
    {
        public Guid BranchId { get; init; }
    }
}