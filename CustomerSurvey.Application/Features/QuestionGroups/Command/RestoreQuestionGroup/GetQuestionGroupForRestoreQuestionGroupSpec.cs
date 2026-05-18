using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed class GetQuestionGroupForRestoreQuestionGroupSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForRestoreQuestionGroupSpec(
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