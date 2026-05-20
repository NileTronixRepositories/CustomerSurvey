using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed class GetCurrentBranchUserForQuestionsSelectionSpec
        : Specification<BranchUser, CurrentBranchActorForGetAnonymousTemplateQuestionsSelectionDto>
    {
        public GetCurrentBranchUserForQuestionsSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateQuestionsSelectionDto
            {
                BranchId = x.BranchId
            });
        }
    }
}