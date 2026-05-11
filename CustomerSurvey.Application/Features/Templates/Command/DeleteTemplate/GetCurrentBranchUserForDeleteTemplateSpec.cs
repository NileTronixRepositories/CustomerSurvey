using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplate
{
    internal sealed record CurrentBranchUserForDeleteTemplateDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForDeleteTemplateSpec
        : Specification<BranchUser, CurrentBranchUserForDeleteTemplateDto>
    {
        public GetCurrentBranchUserForDeleteTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForDeleteTemplateDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}