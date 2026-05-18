using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

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
                TemplateId = x.TemplateId,
                QuestionId = x.QuestionId,

                QuestionBranchId = x.Question.BranchId,
                GroupId = x.Question.GroupId,
                GroupBranchId = x.Question.Group.BranchId,
                Scope = x.Question.Scope,

                Order = x.Order,
                Type = x.Question.Type
            });
        }
    }
}