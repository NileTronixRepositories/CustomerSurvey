using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed record CurrentBranchActorForRestoreQuestionGroupDto
    {
        public Guid BranchId { get; init; }
    }
}