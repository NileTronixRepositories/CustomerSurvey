using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class GetQuestionForUpdateSpec
         : Specification<Question>
    {
        public GetQuestionForUpdateSpec(
            Guid questionId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.BranchId == branchId);
        }
    }
}