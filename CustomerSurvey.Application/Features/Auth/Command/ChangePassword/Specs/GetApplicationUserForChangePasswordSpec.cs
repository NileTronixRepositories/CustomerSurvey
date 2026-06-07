using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Auth.Command.ChangePassword.Specs
{
    internal sealed class GetApplicationUserForChangePasswordSpec
        : Specification<ApplicationUser>
    {
        public GetApplicationUserForChangePasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.Id == applicationUserId);
        }
    }
}
