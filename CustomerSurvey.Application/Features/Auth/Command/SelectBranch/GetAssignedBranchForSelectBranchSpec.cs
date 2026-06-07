using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    internal sealed class GetAssignedBranchForSelectBranchSpec
        : Specification<BranchAreaBranch, SelectedBranchResponse>
    {
        public GetAssignedBranchForSelectBranchSpec(Guid branchAreaId, Guid branchId)
        {
            AddCriteria(x =>
                x.BranchAreaId == branchAreaId &&
                x.BranchId == branchId &&
                x.Branch.IsActive);

            Select(x => new SelectedBranchResponse
            {
                Id = x.BranchId,
                NameEn = x.Branch.NameEn,
                NameAr = x.Branch.NameAr,
                Code = x.Branch.Code
            });
        }
    }
}
