using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.infrastructure.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Services.Security
{
    public sealed class OtpHasher : IOtpHasher
    {
        private readonly byte[] _secret;

        public OtpHasher(IOptions<OtpOptions> options)
        {
            var secret = options.Value.Secret;

            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Missing config: Security:Otp:Secret");

            _secret = Encoding.UTF8.GetBytes(secret);
        }

        public ValueTask<(string hash, string salt)> HashAsync(
            string code,
            Guid accountId,
            string purpose,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);

            var data = $"{code}|{salt}|{accountId:N}|{purpose}";
            using var hmac = new HMACSHA256(_secret);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            // ✅ tuple names لازم تبقى hash/salt زي الواجهة
            var hash = Convert.ToBase64String(hashBytes);

            return ValueTask.FromResult((hash: hash, salt: salt));
        }

        public ValueTask<bool> VerifyAsync(
            string code,
            Guid accountId,
            string purpose,
            string expectedHash,
            string salt,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var data = $"{code}|{salt}|{accountId:N}|{purpose}";
            using var hmac = new HMACSHA256(_secret);
            var actual = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var expected = Convert.FromBase64String(expectedHash);

            var ok = CryptographicOperations.FixedTimeEquals(actual, expected);
            return ValueTask.FromResult(ok);
        }
    }
}