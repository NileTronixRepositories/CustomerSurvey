using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace BuildingBlock.Api.Middleware
{
    public sealed class ResponseContentLanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseContentLanguageMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            // اضبط الهيدر قبل الإرسال (بأمان)
            context.Response.OnStarting(() =>
            {
                var lang = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName ?? "en";
                context.Response.Headers["Content-Language"] = lang;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}