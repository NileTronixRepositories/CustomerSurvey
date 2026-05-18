using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record TemplateQuestionForTemplateQuestionsSelectionDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }
    }

    internal sealed class GetTemplateQuestionsForTemplateQuestionsSelectionSpec
        : Specification<TemplateQuestion, TemplateQuestionForTemplateQuestionsSelectionDto>
    {
        public GetTemplateQuestionsForTemplateQuestionsSelectionSpec(Guid templateId)
        {
            AddCriteria(x => x.TemplateId == templateId);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionForTemplateQuestionsSelectionDto
            {
                TemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                Order = x.Order
            });
        }
    }
}