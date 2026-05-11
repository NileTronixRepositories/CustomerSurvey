using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs
{
    internal sealed record CurrentQuestionGroupBranchActorDto
    {
        public Guid BranchId { get; init; }
    }
}