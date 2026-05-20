using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class GetCurrentBranchAdminForManageAnonymousTemplateQuestionConditionsSpec
        : Specification<BranchAdmin, CurrentBranchActorForManageAnonymousTemplateQuestionConditionsDto>
    {
        public GetCurrentBranchAdminForManageAnonymousTemplateQuestionConditionsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForManageAnonymousTemplateQuestionConditionsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}