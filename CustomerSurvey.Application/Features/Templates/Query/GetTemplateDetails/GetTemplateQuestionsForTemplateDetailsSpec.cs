using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed class GetTemplateQuestionsForTemplateDetailsSpec
        : Specification<TemplateQuestion, TemplateQuestionForTemplateDetailsDto>
    {
        public GetTemplateQuestionsForTemplateDetailsSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionForTemplateDetailsDto
            {
                TemplateQuestionId = x.Id,
                TemplateId = x.TemplateId,
                QuestionId = x.QuestionId,
                Order = x.Order,
                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,
                Type = x.Question.Type,
                IsActive = x.Question.IsActive,
                GroupId = x.Question.GroupId,
                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr
            });
        }
    }
}