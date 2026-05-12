using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed record CurrentBranchActorForAssignQuestionsToTemplateDto
    {
        public Guid BranchId { get; init; }
    }
}