using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class GetTargetQuestionOptionsForCopyToBranchSpec : Specification<QuestionOption>
{
    public GetTargetQuestionOptionsForCopyToBranchSpec(Guid questionId)
    {
        AddCriteria(x => x.QuestionId == questionId);
        AddOrderBy(x => x.Order);
    }
}
