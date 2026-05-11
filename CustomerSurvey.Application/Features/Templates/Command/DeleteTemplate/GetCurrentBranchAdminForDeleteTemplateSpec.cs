using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplate
{
    internal sealed record CurrentBranchAdminForDeleteTemplateDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForDeleteTemplateSpec
        : Specification<BranchAdmin, CurrentBranchAdminForDeleteTemplateDto>
    {
        public GetCurrentBranchAdminForDeleteTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForDeleteTemplateDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}