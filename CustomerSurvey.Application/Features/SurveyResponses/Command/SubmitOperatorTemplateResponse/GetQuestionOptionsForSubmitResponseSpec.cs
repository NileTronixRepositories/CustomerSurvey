using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetQuestionOptionsForSubmitResponseSpec
        : Specification<QuestionOption, QuestionOptionForSubmitResponseDto>
    {
        public GetQuestionOptionsForSubmitResponseSpec(IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x =>
                questionIds.Contains(x.QuestionId) &&
                x.IsActive);

            Select(x => new QuestionOptionForSubmitResponseDto
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId
            });
        }
    }
}