using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed record CurrentBranchActorForManageTemplateQuestionConditionsDto
    {
        public Guid BranchId { get; init; }
    }
}