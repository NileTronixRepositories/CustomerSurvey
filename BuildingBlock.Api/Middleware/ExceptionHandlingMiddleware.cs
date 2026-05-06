using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System.Net;
using System.Text.Json;

namespace BuildingBlock.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;
                var corr = context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationItemKey, out var cid)
                    ? cid?.ToString()
                    : null;

                using (LogContext.PushProperty("traceId", traceId))
                using (LogContext.PushProperty("correlationId", corr ?? string.Empty))
                {
                    Log.ForContext("IsAudit", true)
                       .Error(ex, "Unhandled exception for {RequestPath} (corr={correlationId}, trace={traceId})",
                            context.Request.Path, corr, traceId);
                }

                // لو الريسبونس بدأ فعلاً، مينفعش نعدل هيدرز/بودي
                if (context.Response.HasStarted)
                {
                    Log.Warning("The response has already started. Cannot write error response (traceId: {TraceId})", traceId);
                    throw; // سيبه يطلع للهوستنج/DevExceptionPage
                }

                var problem = new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                    title = "An unexpected error occurred.",
                    status = (int)HttpStatusCode.InternalServerError,
                    traceId,
                    correlationId = corr,
                    detail = _env.IsDevelopment() ? ex.ToString() : null
                };

                var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/problem+json; charset=utf-8";
                await context.Response.WriteAsync(json);
            }
        }
    }
}