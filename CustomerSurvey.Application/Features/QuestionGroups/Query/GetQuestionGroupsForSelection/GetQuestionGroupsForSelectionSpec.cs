using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    internal sealed class GetQuestionGroupsForSelectionSpec
        : Specification<QuestionGroup, QuestionGroupSelectionResponse>
    {
        public GetQuestionGroupsForSelectionSpec(Guid branchId)
        {
            AddCriteria(x =>
                x.Scope == QuestionScope.Branch &&
                x.BranchId.HasValue &&
                x.BranchId.Value == branchId &&
                x.IsActive);

            AddOrderBy(x => x.NameEn);

            Select(x => new QuestionGroupSelectionResponse
            {
                Id = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == QuestionScope.Global,

                // This endpoint is used for creating/updating Branch Questions only.
                // So only Branch active groups are selectable here.
                IsSelectable = x.Scope == QuestionScope.Branch,

                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}