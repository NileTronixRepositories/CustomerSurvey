using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class GetGlobalQuestionOptionsForUpdateSpec
          : Specification<QuestionOption>
    {
        public GetGlobalQuestionOptionsForUpdateSpec(Guid questionId)
        {
            AddCriteria(x => x.QuestionId == questionId);

            AddOrderBy(x => x.Order);
        }
    }
}