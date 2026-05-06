using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed record UserRoleForLoginDto
    {
        public Guid RoleId { get; init; }
        public string RoleName { get; init; } = string.Empty;
    }

    internal sealed class GetUserRolesForLoginSpec
        : Specification<UserRole, UserRoleForLoginDto>
    {
        public GetUserRolesForLoginSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new UserRoleForLoginDto
            {
                RoleId = x.RoleId,
                RoleName = x.Role.Name
            });
        }
    }
}