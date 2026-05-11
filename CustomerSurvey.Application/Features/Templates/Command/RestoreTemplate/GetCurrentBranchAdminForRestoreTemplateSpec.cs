using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.RestoreTemplate
{
    internal sealed class GetCurrentBranchAdminForRestoreTemplateSpec
        : Specification<BranchAdmin, CurrentBranchActorForRestoreTemplateDto>
    {
        public GetCurrentBranchAdminForRestoreTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForRestoreTemplateDto
            {
                BranchId = x.BranchId
            });
        }
    }
}