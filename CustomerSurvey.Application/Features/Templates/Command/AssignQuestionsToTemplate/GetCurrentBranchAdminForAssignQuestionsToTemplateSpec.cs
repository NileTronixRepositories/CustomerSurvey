using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class GetCurrentBranchAdminForAssignQuestionsToTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForAssignQuestionsToTemplateDto>
    {
        public GetCurrentBranchAdminForAssignQuestionsToTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForAssignQuestionsToTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}