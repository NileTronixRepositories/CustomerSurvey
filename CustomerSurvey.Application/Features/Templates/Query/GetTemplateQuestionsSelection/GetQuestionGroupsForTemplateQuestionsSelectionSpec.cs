using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record QuestionGroupForTemplateQuestionsSelectionDto
    {
        public Guid GroupId { get; init; }

        public Guid? BranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }

    internal sealed class GetQuestionGroupsForTemplateQuestionsSelectionSpec
        : Specification<QuestionGroup, QuestionGroupForTemplateQuestionsSelectionDto>
    {
        public GetQuestionGroupsForTemplateQuestionsSelectionSpec(Guid branchId)
        {
            AddCriteria(x =>
                x.IsActive &&
                (
                    (x.Scope == QuestionScope.Branch && x.BranchId == branchId) ||
                    (x.Scope == QuestionScope.Global && x.BranchId == null)
                ));

            AddOrderBy(x => x.NameEn);

            Select(x => new QuestionGroupForTemplateQuestionsSelectionDto
            {
                GroupId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}