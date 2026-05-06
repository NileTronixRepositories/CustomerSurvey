using BuildingBlock.Application.Abstraction.Security;
using Microsoft.AspNetCore.Http;

namespace BuildingBlock.Application.MultiTenancy
{
    public sealed class TenantContextMiddleware : IMiddleware
    {
        private readonly ITokenReader _tokenReader;
        private readonly CurrentTenantContext _tenant;

        public TenantContextMiddleware(ITokenReader tokenReader, ICurrentTenantContext tenant)
        {
            _tokenReader = tokenReader;
            _tenant = (CurrentTenantContext)tenant;
        }

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var user = context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var info = _tokenReader.ReadFromPrincipal(user);

                _tenant.IsAuthenticated = true;
                _tenant.UserId = info.UserId;
                _tenant.Role = info.Role;
                _tenant.UserType = info.UserType;

                if (info.HasAccount)
                {
                    _tenant.Mode = TenantMode.Account;
                    _tenant.AccountId = info.AccountId!.Value;
                }
                else
                {
                    _tenant.Mode = TenantMode.Platform;
                    _tenant.AccountId = null;
                }
            }
            else
            {
                _tenant.IsAuthenticated = false;
                _tenant.Mode = TenantMode.Platform;
                _tenant.AccountId = null;
                _tenant.UserId = null;
                _tenant.Role = null;
                _tenant.UserType = UserType.Unknown;
            }

            return next(context);
        }
    }
}