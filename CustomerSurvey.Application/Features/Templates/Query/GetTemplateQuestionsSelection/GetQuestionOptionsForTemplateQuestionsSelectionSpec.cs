using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record TemplateQuestionSelectionOptionForReadDto
    {
        public Guid QuestionId { get; init; }

        public Guid OptionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }

    internal sealed class GetQuestionOptionsForTemplateQuestionsSelectionSpec
        : Specification<QuestionOption, TemplateQuestionSelectionOptionForReadDto>
    {
        public GetQuestionOptionsForTemplateQuestionsSelectionSpec(
            IReadOnlyCollection<Guid> questionIds)
        {
            AddCriteria(x =>
                questionIds.Contains(x.QuestionId) &&
                x.IsActive);

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionSelectionOptionForReadDto
            {
                QuestionId = x.QuestionId,
                OptionId = x.Id,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Order = x.Order,
                Value = x.Value
            });
        }
    }
}