using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Auth.Command.Login
{
    internal sealed class GetApplicationUserForLoginSpec : Specification<ApplicationUser>
    {
        public GetApplicationUserForLoginSpec(string userNameOrEmail)
        {
            var value = userNameOrEmail.Trim();

            AddCriteria(x =>
                x.UserName == value ||
                x.Email == value);
        }
    }
}