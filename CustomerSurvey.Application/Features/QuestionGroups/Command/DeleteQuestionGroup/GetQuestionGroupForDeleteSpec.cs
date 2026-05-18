using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.DeleteQuestionGroup
{
    internal sealed class GetQuestionGroupForDeleteSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForDeleteSpec(
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