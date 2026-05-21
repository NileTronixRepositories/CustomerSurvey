using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Infrastracture.Bootstrap;
using BuildingBlock.Infrastracture.Interceptors;
using BuildingBlock.Infrastracture.Options;
using BuildingBlock.Infrastracture.Service;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Abstraction.Seeding;
using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.infrastructure.Authorization;
using CustomerSurvey.infrastructure.BackgroundJobs.Templates;
using CustomerSurvey.infrastructure.Options;
using CustomerSurvey.infrastructure.Persistence;
using CustomerSurvey.infrastructure.Reports;
using CustomerSurvey.infrastructure.Repositories;
using CustomerSurvey.infrastructure.Seeders;
using CustomerSurvey.infrastructure.Services;
using CustomerSurvey.infrastructure.Services.Security;
using CustomerSurvey.infrastructure.Services.Token;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CustomerSurvey.infrastructure.Bootstrap
{
    public static class Bootstrap
    {
        public static IServiceCollection InfrastructureInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<EncryptionOptions>(configuration.GetSection("EncryptionOptions"));
            services.Configure<JwtOption>(configuration.GetSection(JwtOption.SectionName));
            services.AddOptions<OtpOptions>()
          .Bind(configuration.GetSection("Security:Otp"))
          .Validate(o => !string.IsNullOrWhiteSpace(o.Secret),
              "Security:Otp:Secret is required.")
          .ValidateOnStart();
            //Hasing
            services.AddScoped<IOtpHasher, OtpHasher>();
            //Email Service
            services.AddingEmailService(configuration);
            // BuildingBlock core services
            services.AddSecurity();
            services.AddMediaService();
            services.AddQrCodeService();

            // CQRS EF (marker-based registrations: IReadRepository<,> IWriteRepository<,> IUnitOfWork<>)
            services.AddEfInfrastructure();

            // Interceptors (Write side)
            services.AddBuildingBlockAuditingAndSoftDelete();
            services.AddBuildingBlockMultiTenancy();      // TenantContext + Middleware
            services.AddBuildingBlockMultiTenancyEf();    // SaveChanges Interceptor

            // DbContexts (Write + Read)
            services.AddDbConfig(configuration);

            // Marker -> DbContext resolvers
            services.AddScoped<IDbContextResolver<PlatformReadMarker>, PlatformReadDbContextResolver>();
            services.AddScoped<IDbContextResolver<PlatformWriteMarker>, PlatformWriteDbContextResolver>();
            services.AddScoped<IDbContextResolver<PlatformWriteReadMarker>, PlatformWriteReadDbContextResolver>();

            // Platform aliases (no DbContext in Application code)
            services.AddScoped(typeof(IReadRepository<>), typeof(PlatformReadRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(PlatformWriteRepository<>));
            services.AddScoped<IUnitOfWork, PlatformUnitOfWork>();
            services.AddScoped(typeof(IWriteReadRepository<>), typeof(PlatformWriteReadRepository<>));
            services.AddScoped<IWriteDbContextAccessor, PlatformWriteDbContextAccessor>();
            services.AddScoped(typeof(IReadModelWriter<>), typeof(EfReadModelWriter<>));

            // public survey related services
            services.Configure<PublicSurveyOptions>(
    configuration.GetSection(PublicSurveyOptions.SectionName));

            services.AddScoped<IPublicSurveyUrlBuilder, PublicSurveyUrlBuilder>();
            services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();

            // Http + Cache
            services.AddHttpClient();
            services.AddSingleton<ICacheService, MemoryCacheService>();

            // AuthN/AuthZ
            services.AddAuth(configuration);

            // Token reader
            services.AddScoped<ITokenReader, JwtReader>();

            // Seeding + DB init (HostedServices - correct place)
            services.AddSeeding();
            services.AddHostedService<DbInitAndSeedingHostedService>();
            services.AddHostedService<ExpireTemplatesBackgroundService>();

            //Pdf
            services.AddScoped<IRazorViewRenderer, RazorViewRenderer>();
            services.AddScoped<IPdfService, PdfService>();

            services.AddScoped<IBranchTemplatesPdfReportService, BranchTemplatesPdfReportService>();

            return services;
        }

        private static IServiceCollection AddDbConfig(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Database")!;

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A valid database connection string must be provided.");

            // Write DbContext (Domain tables + Interceptors)
            services.AddDbContext<PlatformWriteDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    // optional: separate history table for write
                    sql.MigrationsHistoryTable("__EFMigrationsHistory_Write");
                });
                options.AddBuildingBlockTenantInterceptors(sp);
                options.AddInterceptors(sp.GetRequiredService<SoftDeleteEntitiesInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<AuditableEntitiesInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
                options.AddInterceptors(sp.GetRequiredService<TenantSaveChangesInterceptor>());
            });

            // Read DbContext (ReadModel tables)
            services.AddDbContext<PlatformReadDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sql =>
                {
                    // optional: separate history table for read
                    sql.MigrationsHistoryTable("__EFMigrationsHistory_Read");
                });

                // Usually no interceptors here
            });

            return services;
        }

        #region Seeding Database

        private static IServiceCollection AddSeeding(this IServiceCollection services)
        {
            services.AddScoped<ISeeder, SuperAdminSeeder>();
            services.AddScoped<ISeeder, RoleSeeder>();
            services.AddScoped<ISeeder, PermissionSeeder>();
            services.AddScoped<ISeeder, RolePermissionSeeder>();
            services.AddScoped<ISeeder, SuperAdminRoleSeeder>();

            services.AddScoped<IEnsureSeeding, EnsureSeeding>();

            return services;
        }

        /// <summary>
        /// Runs at startup:
        /// 1) Migrate/ensure DB (Write + Read contexts)
        /// 2) Run seeders (Write side)
        /// </summary>
        private sealed class DbInitAndSeedingHostedService : IHostedService
        {
            private readonly IServiceScopeFactory _scopeFactory;
            private readonly ILogger<DbInitAndSeedingHostedService> _logger;

            public DbInitAndSeedingHostedService(IServiceScopeFactory scopeFactory, ILogger<DbInitAndSeedingHostedService> logger)
            {
                _scopeFactory = scopeFactory;
                _logger = logger;
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                using var scope = _scopeFactory.CreateScope();

                var writeDb = scope.ServiceProvider.GetRequiredService<PlatformWriteDbContext>();
                var readDb = scope.ServiceProvider.GetRequiredService<PlatformReadDbContext>();

                await EnsureDbUpToDateAsync(writeDb, "WRITE", cancellationToken);
                await EnsureDbUpToDateAsync(readDb, "READ", cancellationToken);

                var seeder = scope.ServiceProvider.GetRequiredService<IEnsureSeeding>();
                _logger.LogInformation("Starting database seeding...");
                await seeder.SeedDatabaseAsync();
                _logger.LogInformation("Database seeding completed.");
            }

            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            private async Task EnsureDbUpToDateAsync(DbContext db, string name, CancellationToken ct)
            {
                try
                {
                    _logger.LogInformation("[{Name}] Checking database connectivity...", name);

                    // If you prefer EnsureCreated for dev-only, keep it. In prod, use Migrate.
                    // Most teams: always Migrate.
                    await db.Database.MigrateAsync(ct);

                    _logger.LogInformation("[{Name}] Database migrated / up to date.", name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{Name}] Database initialization failed.", name);
                    throw;
                }
            }
        }

        #endregion Seeding Database

        #region Authentication & Authorization

        private static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtProvider, JwtProvider>();

            var jwtSettings = new JwtOption();
            configuration.GetSection(JwtOption.SectionName).Bind(jwtSettings);

            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                    };
                });

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }

        #endregion Authentication & Authorization
    }
}