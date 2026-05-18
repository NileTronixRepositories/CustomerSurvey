using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

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
                QuestionBranchId = x.Question.BranchId,
                GroupId = x.Question.GroupId,
                GroupBranchId = x.Question.Group.BranchId,
                Scope = x.Question.Scope,
                Order = x.Order,
                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,
                Type = x.Question.Type,
                IsActive = x.Question.IsActive,
                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr
            });
        }
    }
}