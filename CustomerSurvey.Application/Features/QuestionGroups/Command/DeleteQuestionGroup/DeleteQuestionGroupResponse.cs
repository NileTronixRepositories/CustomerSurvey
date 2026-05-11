using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.DeleteQuestionGroup
{
    public sealed record DeleteQuestionGroupResponse
    {
        public Guid GroupId { get; init; }

        public Guid BranchId { get; init; }

        public bool IsActive { get; init; }
    }
}