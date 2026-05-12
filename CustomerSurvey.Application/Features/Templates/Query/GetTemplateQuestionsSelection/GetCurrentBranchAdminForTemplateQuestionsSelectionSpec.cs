using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed class GetCurrentBranchAdminForTemplateQuestionsSelectionSpec
       : Specification<BranchAdmin, CurrentBranchActorForTemplateQuestionsSelectionDto>
    {
        public GetCurrentBranchAdminForTemplateQuestionsSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForTemplateQuestionsSelectionDto
            {
                BranchId = x.BranchId
            });
        }
    }
}