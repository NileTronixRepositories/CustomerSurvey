using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed record CurrentBranchActorForGetAnonymousTemplateDetailsDto
    {
        public Guid BranchId { get; init; }
    }
}