using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection
{
    internal sealed record CurrentBranchUserForTemplatesSelectionDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForTemplatesSelectionSpec
        : Specification<BranchUser, CurrentBranchUserForTemplatesSelectionDto>
    {
        public GetCurrentBranchUserForTemplatesSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForTemplatesSelectionDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}