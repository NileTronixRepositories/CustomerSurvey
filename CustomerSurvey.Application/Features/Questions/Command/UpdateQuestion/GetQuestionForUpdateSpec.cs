using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class GetQuestionForUpdateSpec : Specification<Question>
    {
        public GetQuestionForUpdateSpec(
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