using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.CreateQuestion
{
    internal sealed class GetQuestionGroupForCreateQuestionSpec
        : Specification<QuestionGroup>
    {
        public GetQuestionGroupForCreateQuestionSpec(
            Guid groupId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == groupId &&
                x.Scope == QuestionScope.Branch &&
                x.BranchId == branchId &&
                x.IsActive);
        }
    }
}