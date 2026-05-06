using BuildingBlock.Application.Abstraction.Security;
using CustomerSurvey.infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Services.Token
{
    public sealed class JwtReader : ITokenReader
    {
        private readonly JwtSecurityTokenHandler _handler = new();
        private readonly JwtOption _opt;

        public JwtReader(IOptions<JwtOption> options)
        {
            _opt = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public TokenInfoDto ReadFromPrincipal(ClaimsPrincipal principal)
        {
            // غالباً RawToken مش متاح هنا، هنرجّع اللي نقدر عليه
            return Map(principal, raw: string.Empty, jwt: null);
        }

        public bool TryGetClaim(ClaimsPrincipal principal, string claimType, out string? value)
        {
            value = principal.FindFirstValue(claimType);
            return !string.IsNullOrWhiteSpace(value);
        }

        public (bool ok, T? value) TryGet<T>(ClaimsPrincipal principal, string claimType)
        {
            var s = principal.FindFirstValue(claimType);
            if (string.IsNullOrWhiteSpace(s)) return (false, default);

            try
            {
                object? casted =
                    typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?)
                        ? Guid.Parse(s)
                    : typeof(T) == typeof(int) || typeof(T) == typeof(int?)
                        ? int.Parse(s)
                    : typeof(T) == typeof(long) || typeof(T) == typeof(long?)
                        ? long.Parse(s)
                    : typeof(T) == typeof(bool) || typeof(T) == typeof(bool?)
                        ? bool.Parse(s)
                    : (object?)s;

                return (true, (T?)casted);
            }
            catch
            {
                return (false, default);
            }
        }

        private static string NormalizeBearer(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new SecurityTokenException("Empty token.");

            var trimmed = input.Trim();
            if (trimmed.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return trimmed["Bearer ".Length..].Trim();

            return trimmed;
        }

        private static TokenValidationParameters BuildValidationParameters(JwtOption opt)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.Secret)),

                ValidateIssuer = !string.IsNullOrWhiteSpace(opt.Issuer),
                ValidIssuer = opt.Issuer,

                ValidateAudience = !string.IsNullOrWhiteSpace(opt.Audience),
                ValidAudience = opt.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        }

        private static TokenInfoDto Map(ClaimsPrincipal principal, string raw, JwtSecurityToken? jwt)
        {
            static Guid? GuidOrNull(string? s)
                => Guid.TryParse(s, out var g) ? g : null;

            var permissions = principal.FindAll(JwtClaimTypesCustom.Permission)
                                       .Select(c => c.Value)
                                       .Where(v => !string.IsNullOrWhiteSpace(v))
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToArray();

            return new TokenInfoDto
            {
                RawToken = raw,

                UserId = GuidOrNull(principal.FindFirstValue(JwtClaimTypesCustom.UserId)),
                AccountId = GuidOrNull(principal.FindFirstValue(JwtClaimTypesCustom.AccountId)),

                Email = principal.FindFirstValue(JwtClaimTypesCustom.Email),
                PhoneNumber = principal.FindFirstValue(JwtClaimTypesCustom.PhoneNumber),

                Role = principal.FindFirstValue(ClaimTypes.Role),
                Permissions = permissions,

                Issuer = jwt?.Issuer,
                Audience = jwt?.Audiences?.FirstOrDefault(),
                IssuedAtUtc = jwt?.ValidFrom.ToUniversalTime(),
                ExpiresAtUtc = jwt?.ValidTo.ToUniversalTime(),
                Subject = jwt?.Subject,
                JwtId = jwt?.Id
            };
        }

        TokenInfoDto ITokenReader.ReadFromBearer(string bearerToken)
        {
            var raw = NormalizeBearer(bearerToken);

            var principal = _handler.ValidateToken(
                raw,
                BuildValidationParameters(_opt),
                out var securityToken);

            return Map(principal, raw, securityToken as JwtSecurityToken);
        }
    }
}