using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.UpdateQuestionGroup
{
    internal sealed class GetQuestionGroupForUpdateSpec
         : Specification<QuestionGroup>
    {
        public GetQuestionGroupForUpdateSpec(
            Guid groupId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.BranchId == branchId);
        }
    }
}