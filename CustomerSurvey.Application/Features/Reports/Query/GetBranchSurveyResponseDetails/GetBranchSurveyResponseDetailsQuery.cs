using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails
{
    public sealed record GetBranchSurveyResponseDetailsQuery
     : IQuery<GetBranchSurveyResponseDetailsResponse>
    {
        public Guid SurveyResponseId { get; init; }
    }
}