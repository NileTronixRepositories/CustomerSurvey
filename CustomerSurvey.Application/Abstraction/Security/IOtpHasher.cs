using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Security
{
    public interface IOtpHasher
    {
        ValueTask<(string hash, string salt)> HashAsync(
            string code,
            Guid accountId,
            string purpose,
            CancellationToken ct = default);

        ValueTask<bool> VerifyAsync(
            string code,
            Guid accountId,
            string purpose,
            string expectedHash,
            string salt,
            CancellationToken ct = default);
    }
}