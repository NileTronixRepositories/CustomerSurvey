using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class GetAnonymousTemplateQuestionsForSubmitSpec
        : Specification<AnonymousTemplateQuestion, AnonymousTemplateQuestionForSubmitDto>
    {
        public GetAnonymousTemplateQuestionsForSubmitSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);

            Select(x => new AnonymousTemplateQuestionForSubmitDto
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                QuestionType = x.Question.Type,
                Order = x.Order,
                QuestionIsActive = x.Question.IsActive,
                GroupIsActive = x.Question.Group.IsActive
            });
        }
    }
}