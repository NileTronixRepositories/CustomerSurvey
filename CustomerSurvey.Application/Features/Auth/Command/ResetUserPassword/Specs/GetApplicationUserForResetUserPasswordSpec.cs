using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword.Specs
{
    internal sealed class GetApplicationUserForResetUserPasswordSpec
        : Specification<ApplicationUser>
    {
        public GetApplicationUserForResetUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.Id == applicationUserId);
        }
    }
}
