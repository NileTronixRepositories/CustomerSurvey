using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed record CurrentBranchAdminForTemplatesPaginationDto
    {
        public Guid BranchAdminId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchAdminForTemplatesPaginationSpec
        : Specification<BranchAdmin, CurrentBranchAdminForTemplatesPaginationDto>
    {
        public GetCurrentBranchAdminForTemplatesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAdminForTemplatesPaginationDto
            {
                BranchAdminId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}