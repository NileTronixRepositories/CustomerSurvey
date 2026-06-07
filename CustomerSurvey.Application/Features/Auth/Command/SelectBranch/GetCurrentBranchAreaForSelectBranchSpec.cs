using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.SelectBranch
{
    internal sealed record CurrentBranchAreaForSelectBranchDto
    {
        public Guid BranchAreaId { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public bool IsActive { get; init; }
    }

    internal sealed class GetCurrentBranchAreaForSelectBranchSpec
        : Specification<BranchArea, CurrentBranchAreaForSelectBranchDto>
    {
        public GetCurrentBranchAreaForSelectBranchSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchAreaForSelectBranchDto
            {
                BranchAreaId = x.Id,
                Email = x.ApplicationUser.Email,
                PhoneNumber = x.ApplicationUser.PhoneNumber,
                IsActive = x.ApplicationUser.IsActive
            });
        }
    }
}
