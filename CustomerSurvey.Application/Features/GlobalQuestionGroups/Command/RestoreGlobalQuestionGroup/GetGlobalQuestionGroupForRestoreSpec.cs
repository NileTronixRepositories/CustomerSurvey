using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.RestoreGlobalQuestionGroup
{
    internal sealed class GetGlobalQuestionGroupForRestoreSpec
          : Specification<QuestionGroup>
    {
        public GetGlobalQuestionGroupForRestoreSpec(Guid groupId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.Scope == QuestionScope.Global &&
                x.BranchId == null);
        }
    }
}