using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    public sealed record GetTemplateDetailsQuery : IQuery<GetTemplateDetailsResponse>
    {
        public Guid TemplateId { get; init; }
    }
}