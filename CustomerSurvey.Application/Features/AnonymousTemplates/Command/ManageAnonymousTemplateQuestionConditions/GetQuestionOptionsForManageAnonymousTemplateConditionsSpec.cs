using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class GetQuestionOptionsForManageAnonymousTemplateConditionsSpec
        : Specification<QuestionOption, QuestionOptionForAnonymousTemplateConditionDto>
    {
        public GetQuestionOptionsForManageAnonymousTemplateConditionsSpec(
            IReadOnlyCollection<Guid> optionIds)
        {
            AddCriteria(x => optionIds.Contains(x.Id));

            Select(x => new QuestionOptionForAnonymousTemplateConditionDto
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId,
                IsActive = x.IsActive
            });
        }
    }
}