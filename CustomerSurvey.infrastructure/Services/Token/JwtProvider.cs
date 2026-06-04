using BuildingBlock.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Dto;
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
    public sealed class JwtProvider : IJwtProvider
    {
        private readonly JwtOption _options;

        public JwtProvider(IOptions<JwtOption> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<UserTokenDto> Generate(
            Guid userId,
            string email,
            string phoneNumber,
            IReadOnlyCollection<string> roleNames,
             CustomerSurvey.Domain.Enums.UserType userType,
            IReadOnlyCollection<string> permissions,
            Guid? activeBranchId = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedRoles = roleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var normalizedPermissions = permissions
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypesCustom.UserId, userId.ToString()),
                new Claim(JwtClaimTypesCustom.UserType, ((int)userType).ToString())
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(new Claim(JwtClaimTypesCustom.Email, email.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                claims.Add(new Claim(JwtClaimTypesCustom.PhoneNumber, phoneNumber.Trim()));
            }

            if (activeBranchId.HasValue && activeBranchId.Value != Guid.Empty)
            {
                claims.Add(new Claim(JwtClaimTypesCustom.ActiveBranchId, activeBranchId.Value.ToString()));
            }

            foreach (var roleName in normalizedRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            foreach (var permission in normalizedPermissions)
            {
                claims.Add(new Claim(JwtClaimTypesCustom.Permission, permission));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Secret));

            var signingCredentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: signingCredentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(new UserTokenDto
            {
                Token = tokenValue,
                UserType = userType.ToString(),
                ActiveBranchId = activeBranchId,
            });
        }
    }
}
