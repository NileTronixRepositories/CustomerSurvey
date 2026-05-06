using BuildingBlock.Application.Abstraction.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Security
{
    public interface IJwtProvider
    {
        Task<UserTokenDto> Generate
            (
            Guid userId,
            Guid? tenantId,
            string email,
            string phoneNumber,
            string roleName,
            UserType userType,
            IReadOnlyCollection<string> permissions,
            CancellationToken cancellationToken = default
            );
    }
}