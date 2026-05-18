using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.UpdateQuestionGroup
{
    internal sealed class GetQuestionGroupForUpdateSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForUpdateSpec(
            Guid groupId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.Scope == QuestionScope.Branch &&
                x.BranchId == branchId);
        }
    }
}