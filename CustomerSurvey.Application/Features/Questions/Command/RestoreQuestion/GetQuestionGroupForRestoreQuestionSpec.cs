using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class GetQuestionGroupForRestoreQuestionSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForRestoreQuestionSpec(
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