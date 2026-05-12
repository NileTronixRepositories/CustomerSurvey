using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record QuestionGroupForTemplateQuestionsSelectionDto
    {
        public Guid GroupId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }

    internal sealed class GetQuestionGroupsForTemplateQuestionsSelectionSpec
        : Specification<QuestionGroup, QuestionGroupForTemplateQuestionsSelectionDto>
    {
        public GetQuestionGroupsForTemplateQuestionsSelectionSpec(Guid branchId)
        {
            AddCriteria(x =>
                x.BranchId == branchId &&
                x.IsActive);

            AddOrderBy(x => x.NameEn);

            Select(x => new QuestionGroupForTemplateQuestionsSelectionDto
            {
                GroupId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}