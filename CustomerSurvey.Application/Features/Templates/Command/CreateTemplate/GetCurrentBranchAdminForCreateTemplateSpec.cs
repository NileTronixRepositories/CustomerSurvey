using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.CreateTemplate
{
    internal sealed record CurrentBranchAdminForCreateTemplateDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForCreateTemplateSpec
        : Specification<BranchAdmin, CurrentBranchAdminForCreateTemplateDto>
    {
        public GetCurrentBranchAdminForCreateTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForCreateTemplateDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}