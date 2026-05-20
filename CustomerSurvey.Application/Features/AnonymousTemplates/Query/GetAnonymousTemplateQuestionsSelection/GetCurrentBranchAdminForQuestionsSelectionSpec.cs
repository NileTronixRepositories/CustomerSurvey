using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetCurrentBranchAdminForQuestionsSelectionSpec
        : Specification<BranchAdmin, CurrentBranchActorForGetAnonymousTemplateQuestionsSelectionDto>
    {
        public GetCurrentBranchAdminForQuestionsSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateQuestionsSelectionDto
            {
                BranchId = x.BranchId
            });
        }
    }
}