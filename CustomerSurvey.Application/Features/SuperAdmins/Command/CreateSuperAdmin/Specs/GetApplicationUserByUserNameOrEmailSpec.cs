using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin.Specs
{
    internal sealed record ApplicationUserByUserNameOrEmailDto
    {
        public Guid ApplicationUserId { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }

    internal sealed class GetApplicationUserByUserNameOrEmailSpec
        : Specification<ApplicationUser, ApplicationUserByUserNameOrEmailDto>
    {
        public GetApplicationUserByUserNameOrEmailSpec(
            string userName,
            string email)
        {
            AddCriteria(x => x.UserName == userName || x.Email == email);

            Select(x => new ApplicationUserByUserNameOrEmailDto
            {
                ApplicationUserId = x.Id,
                UserName = x.UserName,
                Email = x.Email
            });
        }
    }
}
