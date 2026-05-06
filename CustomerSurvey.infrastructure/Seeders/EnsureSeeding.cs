using CustomerSurvey.Application.Abstraction.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Seeders
{
    public class EnsureSeeding : IEnsureSeeding
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EnsureSeeding> _logger;

        public EnsureSeeding(IServiceProvider serviceProvider, ILogger<EnsureSeeding> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SeedDatabaseAsync()
        {
            _logger.LogInformation("Starting database seeding");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scopedProvider = scope.ServiceProvider;

                // جلب الـ Seeders المرتبة حسب ترتيب التنفيذ
                var seeders = scopedProvider.GetServices<ISeeder>()
                    .OrderBy(x => x.ExecutionOrder);

                // تشغيل كل Seeder
                foreach (var seeder in seeders)
                {
                    _logger.LogInformation("Running seeder: {SeederType}", seeder.GetType().Name);
                    await seeder.SeedAsync();
                }

                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database seeding");
                throw;
            }
        }
    }
}