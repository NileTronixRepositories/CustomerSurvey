using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.RestoreGlobalQuestion
{
    internal sealed class GetGlobalQuestionForRestoreSpec
         : Specification<Question>
    {
        public GetGlobalQuestionForRestoreSpec(Guid questionId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.Scope == QuestionScope.Global &&
                x.BranchId == null);
        }
    }
}