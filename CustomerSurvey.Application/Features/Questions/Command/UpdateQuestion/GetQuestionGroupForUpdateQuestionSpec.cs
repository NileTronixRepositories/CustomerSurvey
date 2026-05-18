using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class GetQuestionGroupForUpdateQuestionSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForUpdateQuestionSpec(
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