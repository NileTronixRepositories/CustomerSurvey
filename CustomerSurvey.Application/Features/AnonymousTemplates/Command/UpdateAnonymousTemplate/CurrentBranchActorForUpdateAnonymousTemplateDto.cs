using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    internal sealed record CurrentBranchActorForUpdateAnonymousTemplateDto
    {
        public Guid BranchId { get; init; }
    }
}