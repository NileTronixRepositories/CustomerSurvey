using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Shared.Dto
{
    public sealed record UserTokenDto
    {
        public string Token { get; init; } = string.Empty;


        public string UserType { get; init; } = string.Empty;

        public bool RequiresBranchSelection { get; init; }

        public Guid? ActiveBranchId { get; init; }

        public IReadOnlyCollection<LoginBranchSelectionItemResponse> Branches { get; init; } =
            Array.Empty<LoginBranchSelectionItemResponse>();

        public bool FirstLoginFlag { get; init; }

        public bool PasswordExpiredFlag { get; init; }

        public DateTime? PasswordChangedOnUtc { get; init; }

        public DateTime? PasswordExpiresOnUtc { get; init; }
    }

    public sealed record LoginBranchSelectionItemResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;
    }
}
