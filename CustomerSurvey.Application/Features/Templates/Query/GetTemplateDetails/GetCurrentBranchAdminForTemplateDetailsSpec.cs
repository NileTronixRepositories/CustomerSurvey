using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed record CurrentBranchAdminForTemplateDetailsDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForTemplateDetailsSpec
        : Specification<BranchAdmin, CurrentBranchAdminForTemplateDetailsDto>
    {
        public GetCurrentBranchAdminForTemplateDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForTemplateDetailsDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}