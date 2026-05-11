using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    internal sealed class GetQuestionForDeleteSpec
       : Specification<Question>
    {
        public GetQuestionForDeleteSpec(
            Guid questionId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.BranchId == branchId);
        }
    }
}