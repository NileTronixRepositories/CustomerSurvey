using BuildingBlock.Api.Bootstrap;
using BuildingBlock.Api.Logging;
using BuildingBlock.Api.OpenAi;
using CustomerSurvey.Api.Swagger;
using CustomerSurvey.Application.Bootstrap;
using CustomerSurvey.infrastructure.Bootstrap;
using CustomerSurvey.infrastructure.Options;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationBootstrap();
builder.Services.InfrastructureInjection(builder.Configuration);
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the Bearer token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.OperationFilter<ResultPatternOperationFilter>();

    // Add Accept-Language dropdown to all Swagger endpoints
    c.OperationFilter<AcceptLanguageHeaderOperationFilter>();
});

// ---------Serilog-------- /
builder.AddSerilogBootstrap("NiletronixCrm.Api");
//---------Localization-------- /
builder.Services.AddSharedLocalization(opts =>
{
    opts.SupportedCultures = new[] { "ar", "en" };
    opts.DefaultCulture = "en";
    opts.AllowQueryStringLang = true;
});
builder.Services.AddMemoryCache();
var corsSettings = builder.Configuration
    .GetSection("Cors")
    .Get<CorsSettings>() ?? new CorsSettings();
builder.Services.AddCors(options =>
{
    options.AddPolicy("NiletronixCrm", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod();

        if (corsSettings.AllowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            if (corsSettings.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowCredentials();
            }
            else
            {
                // fallback آمن نسبياً: مفيش origins → معناه block للـ cross-origin
                policy.WithOrigins(Array.Empty<string>());
            }
        }
    });
});
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var app = builder.Build();

app.UseSerilogPipeline();

// HTTPS
app.UseHttpsRedirection();
app.UseHsts();

app.UseStaticFiles();
app.UseRouting();
app.UseCors("NiletronixCrm");

app.UseAuthentication();
app.UseBuildingBlockMultiTenancy();
app.UseAuthorization();

app.UseSharedLocalization();

// Swagger (اختياري حسب الـ Env)
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

// Custom Middleware
app.MapLoggingDiagnostics();

// Controllers
app.MapControllers().RequireCors("NiletronixCrm");

app.Run();