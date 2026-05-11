using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate
{
    internal sealed record CurrentBranchUserForUpdateTemplateDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForUpdateTemplateSpec
        : Specification<BranchUser, CurrentBranchUserForUpdateTemplateDto>
    {
        public GetCurrentBranchUserForUpdateTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForUpdateTemplateDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}