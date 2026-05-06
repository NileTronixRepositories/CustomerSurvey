using BuildingBlock.Application.Abstraction.Encryption;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.QrCode;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Application.Email;
using BuildingBlock.Application.MultiTenancy;
using BuildingBlock.Application.Option;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Application.Time;
using BuildingBlock.Infrastracture.Interceptors;
using BuildingBlock.Infrastracture.Repositories;
using BuildingBlock.Infrastracture.Service;
using BuildingBlock.Infrastracture.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlock.Infrastracture.Bootstrap
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEfInfrastructure(this IServiceCollection services)
        {
            services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));
            services.AddScoped(typeof(IWriteRepository<,>), typeof(EfWriteRepository<,>));
            services.AddScoped(typeof(IUnitOfWork<>), typeof(EfUnitOfWork<>));
            return services;
        }

        public static IServiceCollection AddBuildingBlockAuditing(this IServiceCollection services)
        {
            services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<AuditableEntitiesInterceptor>();
            return services;
        }

        public static IServiceCollection AddBuildingBlockSoftDelete(this IServiceCollection services)
        {
            services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddScoped<SoftDeleteEntitiesInterceptor>();
            return services;
        }

        public static IServiceCollection AddBuildingBlockDomainEvent(this IServiceCollection services)
        {
            services.AddScoped<DomainEventsInterceptor>();
            return services;
        }

        public static IServiceCollection AddBuildingBlockAuditingAndSoftDelete(this IServiceCollection services)
        {
            return services
                .AddBuildingBlockAuditing()
                .AddBuildingBlockSoftDelete()
                .AddBuildingBlockDomainEvent();
        }

        public static IServiceCollection AddingEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
            services.AddScoped<IEmailSender, MailKitEmailSender>();
            return services;
        }

        public static IServiceCollection AddSecurity(this IServiceCollection services)
        {
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            return services;
        }

        public static IServiceCollection AddMediaService(this IServiceCollection services)
        {
            services.AddSingleton<IMediaService, MediaService>();
            return services;
        }

        public static IServiceCollection AddQrCodeService(this IServiceCollection services)
        {
            services.AddSingleton<IQRCodeService, QRCodeService>();
            return services;
        }

        public static IServiceCollection AddBuildingBlockMultiTenancy(this IServiceCollection services)
        {
            services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();
            services.AddSingleton<ITokenReader, TokenReader>();
            services.AddScoped<TenantContextMiddleware>();
            return services;
        }

        public static DbContextOptionsBuilder AddBuildingBlockTenantInterceptors(
            this DbContextOptionsBuilder opt,
            IServiceProvider sp)
        {
            var interceptor = sp.GetRequiredService<TenantSaveChangesInterceptor>();
            return opt.AddInterceptors(interceptor);
        }

        public static IServiceCollection AddBuildingBlockMultiTenancyEf(this IServiceCollection services)
        {
            services.AddScoped<TenantSaveChangesInterceptor>();
            return services;
        }
    }
}