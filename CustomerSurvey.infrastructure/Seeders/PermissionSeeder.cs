using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Seeding;
using CustomerSurvey.Domain.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Seeders
{
    internal sealed class PermissionSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 3;

        private readonly IWriteReadRepository<Permission> _readFromWrite;
        private readonly IWriteRepository<Permission> _write;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PermissionSeeder> _logger;

        public PermissionSeeder(
            IWriteReadRepository<Permission> readFromWrite,
            IWriteRepository<Permission> write,
            IUnitOfWork uow,
            ILogger<PermissionSeeder> logger)
        {
            _readFromWrite = readFromWrite;
            _write = write;
            _uow = uow;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            foreach (var item in SeedConstants.SeedCatalog.Permissions)
            {
                var existsById = await _readFromWrite.AnyAsync(x => x.Id == item.Id);

                if (existsById)
                {
                    _logger.LogInformation("Permission {PermissionName} already exists by Id. Skipping.", item.Name);
                    continue;
                }

                var existsByName = await _readFromWrite.AnyAsync(x => x.Name == item.Name);

                if (existsByName)
                {
                    throw new InvalidOperationException(
                        $"Permission '{item.Name}' already exists with a different Id. Align seed ids before continuing.");
                }

                var permission = Permission.CreateSeeded(
                    id: item.Id,
                    name: item.Name,
                    createdByApplicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId);

                await _write.AddAsync(permission);

                _logger.LogInformation("Permission {PermissionName} seeded successfully.", item.Name);
            }

            await _uow.SaveChangesAsync();
        }
    }
}