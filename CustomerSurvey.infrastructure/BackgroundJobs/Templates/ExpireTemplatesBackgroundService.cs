using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.BackgroundJobs.Templates
{
    internal sealed class ExpireTemplatesBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Period = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpireTemplatesBackgroundService> _logger;

        public ExpireTemplatesBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ExpireTemplatesBackgroundService> logger)
        {
            _scopeFactory = scopeFactory
                ?? throw new ArgumentNullException(nameof(scopeFactory));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExpireTemplatesAsync(stoppingToken);

            using var timer = new PeriodicTimer(Period);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExpireTemplatesAsync(stoppingToken);
            }
        }

        private async Task ExpireTemplatesAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var templateReadRepository = scope.ServiceProvider
                    .GetRequiredService<IWriteReadRepository<Template>>();

                var templateWriteRepository = scope.ServiceProvider
                    .GetRequiredService<IWriteRepository<Template>>();

                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                var utcNow = DateTime.UtcNow;

                var expiredTemplates = await templateReadRepository.ListAsync(
                    new GetExpiredActiveTemplatesSpec(utcNow),
                    cancellationToken);

                if (expiredTemplates.Count == 0)
                {
                    return;
                }

                foreach (var template in expiredTemplates)
                {
                    template.Deactivate();
                    templateWriteRepository.Update(template);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Expired {TemplatesCount} templates at {UtcNow}.",
                    expiredTemplates.Count,
                    utcNow);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire templates.");
            }
        }
    }
}