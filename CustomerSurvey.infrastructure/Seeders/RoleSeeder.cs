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
    internal sealed class RoleSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 2;

        private readonly IWriteReadRepository<Role> _readFromWrite;
        private readonly IWriteRepository<Role> _write;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(
            IWriteReadRepository<Role> readFromWrite,
            IWriteRepository<Role> write,
            IUnitOfWork uow,
            ILogger<RoleSeeder> logger)
        {
            _readFromWrite = readFromWrite;
            _write = write;
            _uow = uow;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            foreach (var item in SeedConstants.SeedCatalog.Roles)
            {
                var existsById = await _readFromWrite.AnyAsync(x => x.Id == item.Id);

                if (existsById)
                {
                    _logger.LogInformation("Role {RoleName} already exists by Id. Skipping.", item.Name);
                    continue;
                }

                var existsByName = await _readFromWrite.AnyAsync(x => x.Name == item.Name);

                if (existsByName)
                {
                    throw new InvalidOperationException(
                        $"Role '{item.Name}' already exists with a different Id. Align seed ids before continuing.");
                }

                var role = Role.CreateSeeded(
                    id: item.Id,
                    name: item.Name,
                    createdByApplicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId);

                await _write.AddAsync(role);

                _logger.LogInformation("Role {RoleName} seeded successfully.", item.Name);
            }

            await _uow.SaveChangesAsync();
        }
    }
}