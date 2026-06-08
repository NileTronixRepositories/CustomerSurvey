using BuildingBlock.Application.Abstraction.Caching;
using BuildingBlock.Application.Behaviors;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Application.Shared.BranchScope;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Bootstrap
{
    public static class Bootstrap
    {
        //Mediator Injection
        private static IServiceCollection AddMediatorInjection(this IServiceCollection services)
        {
            services.AddSingleton<KeyedSemaphore>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(TracingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ExceptionMappingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(QueryCacheBehavior<,>));
                cfg.AddOpenBehavior(typeof(CommandCacheInvalidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(ResilienceBehavior<,>));
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });
            return services;
        }

        private static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
            services.AddValidatorsFromAssembly(
                   AssemblyReference.Assembly,
                   includeInternalTypes: true);
            return services;
        }

        public static IServiceCollection AddApplicationBootstrap(this IServiceCollection services)
        {
            services.AddFluentValidation();
            services.AddScoped<ICurrentBranchScopeResolver, CurrentBranchScopeResolver>();
            services.AddScoped<ISurveyReportScoringService, SurveyReportScoringService>();
            services.AddMediatorInjection();
            return services;
        }
    }
}
