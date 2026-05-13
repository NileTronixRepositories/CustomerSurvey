using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class GetTemplateQuestionsForSubmitResponseSpec
      : Specification<TemplateQuestion, TemplateQuestionForSubmitResponseDto>
    {
        public GetTemplateQuestionsForSubmitResponseSpec(Guid templateId)
        {
            AddCriteria(x =>
                x.TemplateId == templateId &&
                x.Question.IsActive &&
                x.Question.Group.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionForSubmitResponseDto
            {
                TemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                Type = x.Question.Type,
                Order = x.Order
            });
        }
    }
}