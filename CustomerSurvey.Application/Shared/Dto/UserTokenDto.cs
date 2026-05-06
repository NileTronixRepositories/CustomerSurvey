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
        public string RoleName { get; init; } = string.Empty;
        public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
    }
}