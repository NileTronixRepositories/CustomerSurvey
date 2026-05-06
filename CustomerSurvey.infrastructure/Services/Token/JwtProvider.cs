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
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOption _options;

        public JwtProvider(IOptions<JwtOption> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<UserTokenDto> Generate(
            Guid userId,
            Guid? tenantId,
            string email,
            string phoneNumber,
            string roleName,
            Domain.Enums.UserType userType,
            IReadOnlyCollection<string> permissions,
            CancellationToken cancellationToken = default)
        {
            var claims = new List<Claim>();

            claims.Add(new Claim(JwtClaimTypesCustom.UserId, userId.ToString()));
            if (tenantId is not null)
                claims.Add(new Claim(JwtClaimTypesCustom.AccountId, tenantId.Value.ToString()));

            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new Claim(JwtClaimTypesCustom.Email, email));

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                claims.Add(new Claim(JwtClaimTypesCustom.PhoneNumber, phoneNumber));

            if (!string.IsNullOrWhiteSpace(roleName))
                claims.Add(new Claim(ClaimTypes.Role, roleName));

            claims.Add(new Claim(JwtClaimTypesCustom.UserType, ((int)userType).ToString()));

            if (permissions is not null)
            {
                foreach (var p in permissions.Where(x => !string.IsNullOrWhiteSpace(x))
                                             .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    claims.Add(new Claim(JwtClaimTypesCustom.Permission, p));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: null,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: signingCredentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(new UserTokenDto
            {
                Token = tokenValue,
                RoleName = roleName ?? string.Empty,
                Permissions = permissions ?? Array.Empty<string>()
            });
        }
    }
}