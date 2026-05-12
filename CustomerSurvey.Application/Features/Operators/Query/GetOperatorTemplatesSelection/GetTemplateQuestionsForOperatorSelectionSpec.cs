using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    internal sealed record TemplateQuestionForOperatorSelectionDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid TemplateId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }
    }

    internal sealed class GetTemplateQuestionsForOperatorSelectionSpec
        : Specification<TemplateQuestion, TemplateQuestionForOperatorSelectionDto>
    {
        public GetTemplateQuestionsForOperatorSelectionSpec(IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x => templateIds.Contains(x.TemplateId));

            AddOrderBy(x => x.Order);

            Select(x => new TemplateQuestionForOperatorSelectionDto
            {
                TemplateQuestionId = x.Id,
                TemplateId = x.TemplateId,
                QuestionId = x.QuestionId,
                Order = x.Order,
                TextEn = x.Question.TextEn,
                TextAr = x.Question.TextAr,
                Type = x.Question.Type.ToString(),
                IsActive = x.Question.IsActive,
                GroupId = x.Question.GroupId,
                GroupNameEn = x.Question.Group.NameEn,
                GroupNameAr = x.Question.Group.NameAr
            });
        }
    }
}