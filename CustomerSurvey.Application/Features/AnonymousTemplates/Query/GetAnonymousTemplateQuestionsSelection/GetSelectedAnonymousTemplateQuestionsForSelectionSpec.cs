using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetSelectedAnonymousTemplateQuestionsForSelectionSpec
        : Specification<AnonymousTemplateQuestion, SelectedAnonymousTemplateQuestionForSelectionDto>
    {
        public GetSelectedAnonymousTemplateQuestionsForSelectionSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId);

            Select(x => new SelectedAnonymousTemplateQuestionForSelectionDto
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                Order = x.Order
            });
        }
    }
}