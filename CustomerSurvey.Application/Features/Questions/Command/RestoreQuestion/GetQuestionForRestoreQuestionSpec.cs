using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class GetQuestionForRestoreQuestionSpec
       : Specification<Question>
    {
        public GetQuestionForRestoreQuestionSpec(
            Guid questionId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.BranchId == branchId);
        }
    }
}