using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed class GetQuestionGroupForRestoreQuestionGroupSpec
     : Specification<QuestionGroup>
    {
        public GetQuestionGroupForRestoreQuestionGroupSpec(
            Guid groupId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.BranchId == branchId);
        }
    }
}