using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion
{
    internal sealed class GetGlobalQuestionForUpdateSpec : Specification<Question>
    {
        public GetGlobalQuestionForUpdateSpec(Guid questionId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.Scope == QuestionScope.Global &&
                x.BranchId == null);
        }
    }
}