using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Shared.Dto;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed class GetBranchAreaBranchesForLoginSpec
        : Specification<BranchAreaBranch, LoginBranchSelectionItemResponse>
    {
        public GetBranchAreaBranchesForLoginSpec(Guid branchAreaId)
        {
            AddCriteria(x =>
                x.BranchAreaId == branchAreaId &&
                x.Branch.IsActive);

            Select(x => new LoginBranchSelectionItemResponse
            {
                Id = x.BranchId,
                NameEn = x.Branch.NameEn,
                NameAr = x.Branch.NameAr,
                Code = x.Branch.Code
            });
        }
    }
}
