using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record CurrentBranchActorForTemplateQuestionsSelectionDto
    {
        public Guid BranchId { get; init; }
    }
}