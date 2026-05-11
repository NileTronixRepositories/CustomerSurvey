using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed record CurrentBranchUserForTemplatesPaginationDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForTemplatesPaginationSpec
        : Specification<BranchUser, CurrentBranchUserForTemplatesPaginationDto>
    {
        public GetCurrentBranchUserForTemplatesPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForTemplatesPaginationDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}