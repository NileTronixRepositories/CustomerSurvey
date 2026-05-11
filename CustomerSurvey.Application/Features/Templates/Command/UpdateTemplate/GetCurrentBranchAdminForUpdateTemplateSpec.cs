using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed record CurrentBranchAdminForUpdateTemplateDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForUpdateTemplateSpec
        : Specification<BranchAdmin, CurrentBranchAdminForUpdateTemplateDto>
    {
        public GetCurrentBranchAdminForUpdateTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForUpdateTemplateDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}