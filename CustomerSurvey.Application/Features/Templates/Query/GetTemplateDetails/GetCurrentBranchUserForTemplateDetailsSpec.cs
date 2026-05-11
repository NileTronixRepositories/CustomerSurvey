using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    internal sealed record CurrentBranchUserForTemplateDetailsDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForTemplateDetailsSpec
        : Specification<BranchUser, CurrentBranchUserForTemplateDetailsDto>
    {
        public GetCurrentBranchUserForTemplateDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForTemplateDetailsDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}