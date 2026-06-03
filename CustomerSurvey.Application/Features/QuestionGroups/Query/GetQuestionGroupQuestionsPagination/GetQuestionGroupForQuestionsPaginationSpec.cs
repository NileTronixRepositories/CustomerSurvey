using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetQuestionGroupForQuestionsPaginationSpec
        : Specification<QuestionGroup, QuestionGroupForQuestionsPaginationDto>
    {
        public GetQuestionGroupForQuestionsPaginationSpec(
            Guid questionGroupId,
            Guid? branchId)
        {
            AddCriteria(x => x.Id == questionGroupId);

            if (branchId.HasValue)
            {
                AddCriteria(x => x.BranchId == branchId.Value);
            }

            Select(x => new QuestionGroupForQuestionsPaginationDto
            {
                Id = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}
