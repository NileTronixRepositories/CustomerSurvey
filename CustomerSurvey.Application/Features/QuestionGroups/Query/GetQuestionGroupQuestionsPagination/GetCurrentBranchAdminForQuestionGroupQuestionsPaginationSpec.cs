using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetCurrentBranchAdminForQuestionGroupQuestionsPaginationSpec
        : Specification<BranchAdmin, CurrentBranchActorForQuestionGroupQuestionsPaginationDto>
    {
        public GetCurrentBranchAdminForQuestionGroupQuestionsPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForQuestionGroupQuestionsPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }
}
