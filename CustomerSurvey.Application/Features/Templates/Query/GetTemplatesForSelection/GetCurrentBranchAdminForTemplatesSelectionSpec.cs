using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection
{
    internal sealed record CurrentBranchAdminForTemplatesSelectionDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForTemplatesSelectionSpec
        : Specification<BranchAdmin, CurrentBranchAdminForTemplatesSelectionDto>
    {
        public GetCurrentBranchAdminForTemplatesSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForTemplatesSelectionDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}