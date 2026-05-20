using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetCurrentBranchAdminForAnonymousTemplateResponsesPaginationSpec
         : Specification<BranchAdmin, CurrentBranchActorForGetAnonymousTemplateResponsesPaginationDto>
    {
        public GetCurrentBranchAdminForAnonymousTemplateResponsesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplateResponsesPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }
}