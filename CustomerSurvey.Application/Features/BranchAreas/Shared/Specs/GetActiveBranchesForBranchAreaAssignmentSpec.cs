using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.BranchAreas.Shared;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.BranchAreas.Shared.Specs
{
    internal sealed class GetActiveBranchesForBranchAreaAssignmentSpec
        : Specification<Branch, BranchAreaBranchItemResponse>
    {
        public GetActiveBranchesForBranchAreaAssignmentSpec(IReadOnlyCollection<Guid> branchIds)
        {
            AddCriteria(x =>
                branchIds.Contains(x.Id) &&
                x.IsActive);

            Select(x => new BranchAreaBranchItemResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Code = x.Code
            });
        }
    }
}
