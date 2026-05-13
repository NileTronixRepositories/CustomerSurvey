using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Shared.Specs
{
    internal sealed class GetQuestionOptionsForUpdateQuestionSpec
        : Specification<QuestionOption>
    {
        public GetQuestionOptionsForUpdateQuestionSpec(Guid questionId)
        {
            AddCriteria(x => x.QuestionId == questionId);
        }
    }
}