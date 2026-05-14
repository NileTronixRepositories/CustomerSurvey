using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed class GetCurrentBranchAdminForManageTemplateQuestionConditionsSpec
          : Specification<BranchAdmin, CurrentBranchActorForManageTemplateQuestionConditionsDto>
    {
        public GetCurrentBranchAdminForManageTemplateQuestionConditionsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForManageTemplateQuestionConditionsDto
            {
                BranchId = x.BranchId
            });
        }
    }
}