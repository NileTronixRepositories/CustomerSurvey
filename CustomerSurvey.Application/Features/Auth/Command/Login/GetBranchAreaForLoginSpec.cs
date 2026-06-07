using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed record BranchAreaForLoginDto
    {
        public Guid BranchAreaId { get; init; }
    }

    internal sealed class GetBranchAreaForLoginSpec
        : Specification<BranchArea, BranchAreaForLoginDto>
    {
        public GetBranchAreaForLoginSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new BranchAreaForLoginDto
            {
                BranchAreaId = x.Id
            });
        }
    }
}
