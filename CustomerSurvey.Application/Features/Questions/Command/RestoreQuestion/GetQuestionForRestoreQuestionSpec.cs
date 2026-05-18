using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class GetQuestionForRestoreSpec : Specification<Question>
    {
        public GetQuestionForRestoreSpec(
            Guid questionId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == questionId &&
                x.Scope == QuestionScope.Branch &&
                x.BranchId == branchId);
        }
    }
}