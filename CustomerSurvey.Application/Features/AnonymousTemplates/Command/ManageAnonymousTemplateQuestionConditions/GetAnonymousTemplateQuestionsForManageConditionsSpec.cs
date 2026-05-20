using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class GetAnonymousTemplateQuestionsForManageConditionsSpec
        : Specification<AnonymousTemplateQuestion, AnonymousTemplateQuestionForConditionDto>
    {
        public GetAnonymousTemplateQuestionsForManageConditionsSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);

            Select(x => new AnonymousTemplateQuestionForConditionDto
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                QuestionType = x.Question.Type,
                QuestionIsActive = x.Question.IsActive
            });
        }
    }
}