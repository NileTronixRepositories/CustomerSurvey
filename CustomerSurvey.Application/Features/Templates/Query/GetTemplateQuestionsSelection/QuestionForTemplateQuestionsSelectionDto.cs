using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record QuestionForTemplateQuestionsSelectionDto
    {
        public Guid QuestionId { get; init; }

        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }
    }

    internal sealed class GetQuestionsForTemplateQuestionsSelectionSpec
        : Specification<Question, QuestionForTemplateQuestionsSelectionDto>
    {
        public GetQuestionsForTemplateQuestionsSelectionSpec(Guid branchId)
        {
            AddCriteria(x =>
                x.BranchId == branchId &&
                x.IsActive);

            AddOrderBy(x => x.TextEn);

            Select(x => new QuestionForTemplateQuestionsSelectionDto
            {
                QuestionId = x.Id,
                GroupId = x.GroupId,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Type = x.Type
            });
        }
    }
}