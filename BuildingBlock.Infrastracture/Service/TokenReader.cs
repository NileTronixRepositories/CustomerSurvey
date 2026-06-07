using BuildingBlock.Application.Abstraction.Security;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BuildingBlock.Infrastracture.Service
{
    public sealed class TokenReader : ITokenReader
    {
        private static readonly JwtSecurityTokenHandler Handler = new();

        public TokenInfoDto ReadFromBearer(string bearerToken)
        {
            if (string.IsNullOrWhiteSpace(bearerToken))
                throw new ArgumentException("Bearer token is required.", nameof(bearerToken));

            var raw = StripBearerPrefix(bearerToken);

            if (!Handler.CanReadToken(raw))
                throw new FormatException("Invalid JWT token format.");

            // ⚠️ IMPORTANT: ReadJwtToken does NOT validate signature/expiry.
            // In production, prefer ReadFromPrincipal where ASP.NET already validated the token.
            var jwt = Handler.ReadJwtToken(raw);

            var identity = new ClaimsIdentity(jwt.Claims, authenticationType: "Bearer");
            var principal = new ClaimsPrincipal(identity);

            var dto = ReadFromPrincipal(principal);
            dto.RawToken = raw;

            // Fill standard JWT metadata (best-effort)
            dto.Issuer = jwt.Issuer;
            dto.Audience = jwt.Audiences?.FirstOrDefault();
            dto.Subject = jwt.Subject;
            dto.JwtId = jwt.Id;

            dto.IssuedAtUtc = TryGetJwtTimeUtc(jwt, JwtRegisteredClaimNames.Iat);
            dto.ExpiresAtUtc = TryGetJwtTimeUtc(jwt, JwtRegisteredClaimNames.Exp);

            return dto;
        }

        public TokenInfoDto ReadFromPrincipal(ClaimsPrincipal principal)
        {
            if (principal is null)
                throw new ArgumentNullException(nameof(principal));

            var dto = new TokenInfoDto();

            dto.UserId =
                TryGetGuid(principal, JwtClaimTypesCustom.UserId)
                ?? TryGetGuid(principal, ClaimTypes.NameIdentifier)
                ?? TryGetGuid(principal, JwtRegisteredClaimNames.Sub);

            dto.AccountId =
                TryGetGuid(principal, JwtClaimTypesCustom.AccountId);

            dto.ActiveBranchId =
                TryGetGuid(principal, JwtClaimTypesCustom.ActiveBranchId);

            dto.Email =
                TryGetString(principal, JwtClaimTypesCustom.Email)
                ?? TryGetString(principal, ClaimTypes.Email);

            dto.PhoneNumber =
                TryGetString(principal, JwtClaimTypesCustom.PhoneNumber);

            dto.Role =
                TryGetString(principal, JwtClaimTypesCustom.Role)
                ?? TryGetString(principal, ClaimTypes.Role);

            dto.UserType =
                TryGetUserType(principal, JwtClaimTypesCustom.UserType);

            dto.UserTypeValue =
                TryGetInt(principal, JwtClaimTypesCustom.UserType);

            dto.Permissions =
                GetDistinctClaimValues(principal, JwtClaimTypesCustom.Permission);

            // Optional: If your issuer/audience/etc are mapped as claims
            dto.Issuer = TryGetString(principal, "iss");
            dto.Audience = TryGetString(principal, "aud");
            dto.Subject = TryGetString(principal, "sub");
            dto.JwtId = TryGetString(principal, "jti");

            dto.IssuedAtUtc = TryGetDateTimeUtcFromEpoch(principal, "iat");
            dto.ExpiresAtUtc = TryGetDateTimeUtcFromEpoch(principal, "exp");

            return dto;
        }

        public bool TryGetClaim(ClaimsPrincipal principal, string claimType, out string? value)
        {
            if (principal is null) throw new ArgumentNullException(nameof(principal));
            if (string.IsNullOrWhiteSpace(claimType)) throw new ArgumentException("Claim type is required.", nameof(claimType));

            value = GetClaimValueAny(principal, claimType);
            return !string.IsNullOrWhiteSpace(value);
        }

        public (bool ok, T? value) TryGet<T>(ClaimsPrincipal principal, string claimType)
        {
            if (principal is null) throw new ArgumentNullException(nameof(principal));
            if (string.IsNullOrWhiteSpace(claimType)) throw new ArgumentException("Claim type is required.", nameof(claimType));

            var raw = GetClaimValueAny(principal, claimType);
            if (string.IsNullOrWhiteSpace(raw))
                return (false, default);

            if (TryConvert<T>(raw, out var val))
                return (true, val);

            return (false, default);
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static string StripBearerPrefix(string bearer)
        {
            const string prefix = "Bearer ";
            if (bearer.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return bearer[prefix.Length..].Trim();

            return bearer.Trim();
        }

        private static string? GetClaimValueAny(ClaimsPrincipal principal, params string[] types)
        {
            // exact match first
            foreach (var t in types)
            {
                var c = principal.FindFirst(t);
                if (c is not null && !string.IsNullOrWhiteSpace(c.Value))
                    return c.Value;
            }

            // case-insensitive fallback (handles "userId" vs "userid" etc.)
            var set = new HashSet<string>(types, StringComparer.OrdinalIgnoreCase);
            var ci = principal.Claims.FirstOrDefault(c => set.Contains(c.Type));
            return string.IsNullOrWhiteSpace(ci?.Value) ? null : ci!.Value;
        }

        private static string[] GetDistinctClaimValues(ClaimsPrincipal principal, string claimType)
        {
            var list = principal.FindAll(claimType).Select(x => x.Value).ToList();

            // case-insensitive fallback if nothing found
            if (list.Count == 0)
            {
                list = principal.Claims
                    .Where(c => string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.Value)
                    .ToList();
            }

            return list
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string? TryGetString(ClaimsPrincipal principal, string claimType)
            => GetClaimValueAny(principal, claimType);

        private static Guid? TryGetGuid(ClaimsPrincipal principal, string claimType)
        {
            var raw = GetClaimValueAny(principal, claimType);
            if (Guid.TryParse(raw, out var g) && g != Guid.Empty)
                return g;

            return null;
        }

        private static int? TryGetInt(ClaimsPrincipal principal, string claimType)
        {
            var raw = GetClaimValueAny(principal, claimType);
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return value;

            return null;
        }

        private static DateTime? TryGetDateTimeUtcFromEpoch(ClaimsPrincipal principal, string claimType)
        {
            var raw = GetClaimValueAny(principal, claimType);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            // JWT iat/exp are usually seconds since epoch
            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

            return null;
        }

        private static DateTime? TryGetJwtTimeUtc(JwtSecurityToken jwt, string claimType)
        {
            var raw = jwt.Claims.FirstOrDefault(c => string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

            return null;
        }

        private static UserType TryGetUserType(ClaimsPrincipal principal, string claimType)
        {
            var raw = GetClaimValueAny(principal, claimType);
            if (string.IsNullOrWhiteSpace(raw))
                return UserType.Unknown;

            // allow numeric
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                && Enum.IsDefined(typeof(UserType), i))
            {
                return (UserType)i;
            }

            // allow enum name
            if (Enum.TryParse<UserType>(raw, ignoreCase: true, out var parsed))
                return parsed;

            return UserType.Unknown;
        }

        private static bool TryConvert<T>(string raw, out T? value)
        {
            value = default;

            var targetType = typeof(T);
            var isNullable = Nullable.GetUnderlyingType(targetType) is not null;
            var nonNullType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                object? result;

                if (nonNullType == typeof(string))
                {
                    result = raw;
                }
                else if (nonNullType == typeof(Guid))
                {
                    if (!Guid.TryParse(raw, out var g))
                        return false;

                    result = g;
                }
                else if (nonNullType.IsEnum)
                {
                    // numeric or name
                    if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    {
                        if (!Enum.IsDefined(nonNullType, i))
                            return false;

                        result = Enum.ToObject(nonNullType, i);
                    }
                    else
                    {
                        if (!Enum.TryParse(nonNullType, raw, ignoreCase: true, out var enumObj))
                            return false;

                        result = enumObj;
                    }
                }
                else if (nonNullType == typeof(DateTime))
                {
                    // support epoch seconds too
                    if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sec))
                        result = DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;
                    else if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                        result = dt;
                    else
                        return false;
                }
                else if (nonNullType == typeof(DateTimeOffset))
                {
                    if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sec))
                        result = DateTimeOffset.FromUnixTimeSeconds(sec);
                    else if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
                        result = dto;
                    else
                        return false;
                }
                else
                {
                    // primitives: int/long/bool/double/decimal...
                    result = Convert.ChangeType(raw, nonNullType, CultureInfo.InvariantCulture);
                }

                value = (T?)result;
                return true;
            }
            catch
            {
                // if T is nullable, empty can be treated as null
                if (isNullable && string.IsNullOrWhiteSpace(raw))
                    return true;

                return false;
            }
        }
    }
}
