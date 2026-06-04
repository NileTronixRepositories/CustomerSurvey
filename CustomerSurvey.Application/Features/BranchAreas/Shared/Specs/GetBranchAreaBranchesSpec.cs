using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.BranchAreas.Shared;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Shared.Specs
{
    internal sealed class GetBranchAreaBranchesSpec
        : Specification<BranchAreaBranch, BranchAreaBranchItemResponse>
    {
        public GetBranchAreaBranchesSpec(Guid branchAreaId)
        {
            AddCriteria(x => x.BranchAreaId == branchAreaId);

            Select(x => new BranchAreaBranchItemResponse
            {
                Id = x.BranchId,
                NameEn = x.Branch.NameEn,
                NameAr = x.Branch.NameAr,
                Code = x.Branch.Code
            });
        }
    }
}
