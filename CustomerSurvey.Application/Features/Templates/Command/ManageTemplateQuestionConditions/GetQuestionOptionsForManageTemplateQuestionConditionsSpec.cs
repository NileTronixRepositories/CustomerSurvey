using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed record QuestionOptionForManageTemplateQuestionConditionsDto
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }
    }

    internal sealed class GetQuestionOptionsForManageTemplateQuestionConditionsSpec
        : Specification<QuestionOption, QuestionOptionForManageTemplateQuestionConditionsDto>
    {
        public GetQuestionOptionsForManageTemplateQuestionConditionsSpec(
            IReadOnlyCollection<Guid> optionIds)
        {
            AddCriteria(x =>
                optionIds.Contains(x.Id) &&
                x.IsActive);

            Select(x => new QuestionOptionForManageTemplateQuestionConditionsDto
            {
                OptionId = x.Id,
                QuestionId = x.QuestionId
            });
        }
    }
}