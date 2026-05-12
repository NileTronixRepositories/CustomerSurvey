using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed class GetCurrentBranchUserForAssignQuestionsToTemplateSpec
          : Specification<BranchUser, CurrentBranchActorForAssignQuestionsToTemplateDto>
    {
        public GetCurrentBranchUserForAssignQuestionsToTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForAssignQuestionsToTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}