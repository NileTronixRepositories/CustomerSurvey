using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreaDetails
{
    internal sealed record BranchAreaDetailsFlatDto
    {
        public Guid BranchAreaId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    internal sealed class GetBranchAreaDetailsSpec
        : Specification<BranchArea, BranchAreaDetailsFlatDto>
    {
        public GetBranchAreaDetailsSpec(Guid branchAreaId)
        {
            AddCriteria(x => x.Id == branchAreaId);

            Select(x => new BranchAreaDetailsFlatDto
            {
                BranchAreaId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                NameEn = x.ApplicationUser.NameEn,
                NameAr = x.ApplicationUser.NameAr,
                UserName = x.ApplicationUser.UserName,
                Email = x.ApplicationUser.Email,
                PhoneNumber = x.ApplicationUser.PhoneNumber,
                IsActive = x.ApplicationUser.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}
