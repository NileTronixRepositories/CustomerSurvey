using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    internal sealed record CurrentBranchActorForCreateAnonymousTemplateDto
    {
        public Guid BranchId { get; init; }
    }
}