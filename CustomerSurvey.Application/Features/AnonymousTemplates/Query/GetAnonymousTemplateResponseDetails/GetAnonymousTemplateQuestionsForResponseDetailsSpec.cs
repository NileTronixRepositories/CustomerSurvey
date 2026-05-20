using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousTemplateQuestionsForResponseDetailsSpec
        : Specification<AnonymousTemplateQuestion, AnonymousTemplateQuestionForResponseDetailsDto>
    {
        public GetAnonymousTemplateQuestionsForResponseDetailsSpec(
            Guid anonymousTemplateId,
            IReadOnlyCollection<Guid> anonymousTemplateQuestionIds)
        {
            AddCriteria(x =>
                x.AnonymousTemplateId == anonymousTemplateId &&
                anonymousTemplateQuestionIds.Contains(x.Id));

            Select(x => new AnonymousTemplateQuestionForResponseDetailsDto
            {
                AnonymousTemplateQuestionId = x.Id,
                QuestionId = x.QuestionId,
                QuestionTextEn = x.Question.TextEn,
                QuestionTextAr = x.Question.TextAr,
                QuestionType = x.Question.Type,
                QuestionOrder = x.Order
            });
        }
    }
}