using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetCurrentBranchUserForQuestionGroupQuestionsPaginationSpec
        : Specification<BranchUser, CurrentBranchActorForQuestionGroupQuestionsPaginationDto>
    {
        public GetCurrentBranchUserForQuestionGroupQuestionsPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForQuestionGroupQuestionsPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }
}
