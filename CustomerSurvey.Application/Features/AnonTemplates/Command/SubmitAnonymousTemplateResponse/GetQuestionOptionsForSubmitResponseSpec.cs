using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class GetQuestionOptionsForSubmitResponseSpec
        : Specification<QuestionOption, QuestionOptionForSubmitDto>
    {
        public GetQuestionOptionsForSubmitResponseSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x => questionIds.Contains(x.QuestionId));

            Select(x => new QuestionOptionForSubmitDto
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                Value = x.Value,
                IsActive = x.IsActive
            });
        }
    }
}