using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record QuestionForTemplateQuestionsSelectionDto
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public QuestionScope Scope { get; init; }

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
                x.IsActive &&
                x.Group.IsActive &&
                (
                    (x.Scope == QuestionScope.Branch && x.BranchId == branchId) ||
                    (x.Scope == QuestionScope.Global && x.BranchId == null)
                ));

            AddOrderBy(x => x.TextEn);

            Select(x => new QuestionForTemplateQuestionsSelectionDto
            {
                QuestionId = x.Id,
                BranchId = x.BranchId,
                GroupId = x.GroupId,
                Scope = x.Scope,
                TextEn = x.TextEn,
                TextAr = x.TextAr,
                Type = x.Type
            });
        }
    }
}