using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    internal sealed class GetCurrentBranchAdminForGetAnonymousTemplatesPaginationSpec
         : Specification<BranchAdmin, CurrentBranchActorForGetAnonymousTemplatesPaginationDto>
    {
        public GetCurrentBranchAdminForGetAnonymousTemplatesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForGetAnonymousTemplatesPaginationDto
            {
                BranchId = x.BranchId
            });
        }
    }
}