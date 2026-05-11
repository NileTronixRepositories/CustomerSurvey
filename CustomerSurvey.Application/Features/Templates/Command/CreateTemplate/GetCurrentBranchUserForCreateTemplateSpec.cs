using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.CreateTemplate
{
    internal sealed record CurrentBranchUserForCreateTemplateDto
    {
        public Guid BranchUserId { get; init; }

        public Guid BranchId { get; init; }
    }

    internal sealed class GetCurrentBranchUserForCreateTemplateSpec
        : Specification<BranchUser, CurrentBranchUserForCreateTemplateDto>
    {
        public GetCurrentBranchUserForCreateTemplateSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForCreateTemplateDto
            {
                BranchUserId = x.Id,
                BranchId = x.BranchId
            });
        }
    }
}