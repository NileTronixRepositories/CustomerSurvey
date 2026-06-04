using CustomerSurvey.Application.Shared.Dto;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Security
{
    public interface IJwtProvider
    {
        Task<UserTokenDto> Generate(
      Guid userId,
      string email,
      string phoneNumber,
      IReadOnlyCollection<string> roleNames,
      UserType userType,
      IReadOnlyCollection<string> permissions,
      Guid? activeBranchId = null,
      CancellationToken cancellationToken = default);
    }
}
