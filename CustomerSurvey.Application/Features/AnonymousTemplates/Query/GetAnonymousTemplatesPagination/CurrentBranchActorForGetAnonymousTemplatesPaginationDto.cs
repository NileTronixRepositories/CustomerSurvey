using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    internal sealed record CurrentBranchActorForGetAnonymousTemplatesPaginationDto
    {
        public Guid BranchId { get; init; }
    }
}