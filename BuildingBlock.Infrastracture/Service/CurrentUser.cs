using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace BuildingBlock.Infrastracture.Service
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;
        private readonly ICurrentTenantContext _tenant;

        public CurrentUser(IHttpContextAccessor http, ICurrentTenantContext tenant)
        {
            _http = http;
            _tenant = tenant;
        }

        public bool IsAuthenticated => _tenant.IsAuthenticated;
        public Guid? UserId => _tenant.UserId;
        public Guid? AccountId => _tenant.AccountId;
        public Guid? ActiveBranchId
        {
            get
            {
                var user = _http.HttpContext?.User;
                var s = user?.FindFirst(JwtClaimTypesCustom.ActiveBranchId)?.Value;

                return Guid.TryParse(s, out var branchId) && branchId != Guid.Empty
                    ? branchId
                    : null;
            }
        }

        public string? Role => _tenant.Role;
        public int? UserTypeValue
        {
            get
            {
                var user = _http.HttpContext?.User;
                var s = user?.FindFirst(JwtClaimTypesCustom.UserType)?.Value;

                return int.TryParse(s, out var value)
                    ? value
                    : null;
            }
        }

        public UserType UserType
        {
            get
            {
                var user = _http.HttpContext?.User;
                if (user is null) return UserType.Unknown;

                var s = user.FindFirst(JwtClaimTypesCustom.UserType)?.Value;
                if (string.IsNullOrWhiteSpace(s)) return UserType.Unknown;

                // ندعم رقم أو string
                if (int.TryParse(s, out var n) && Enum.IsDefined(typeof(UserType), n))
                    return (UserType)n;

                if (Enum.TryParse<UserType>(s, ignoreCase: true, out var parsed))
                    return parsed;

                return UserType.Unknown;
            }
        }
    }
}
