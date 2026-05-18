using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    internal sealed class GetQuestionForDeleteSpec : Specification<Question>
    {
        public GetQuestionForDeleteSpec(
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