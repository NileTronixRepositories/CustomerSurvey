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
    internal sealed class RolePermissionSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 4;

        private readonly IWriteReadRepository<Role> _roleReadFromWrite;
        private readonly IWriteReadRepository<Permission> _permissionReadFromWrite;
        private readonly IWriteReadRepository<RolePermission> _rolePermissionReadFromWrite;
        private readonly IWriteRepository<RolePermission> _rolePermissionWrite;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<RolePermissionSeeder> _logger;

        public RolePermissionSeeder(
            IWriteReadRepository<Role> roleReadFromWrite,
            IWriteReadRepository<Permission> permissionReadFromWrite,
            IWriteReadRepository<RolePermission> rolePermissionReadFromWrite,
            IWriteRepository<RolePermission> rolePermissionWrite,
            IUnitOfWork uow,
            ILogger<RolePermissionSeeder> logger)
        {
            _roleReadFromWrite = roleReadFromWrite;
            _permissionReadFromWrite = permissionReadFromWrite;
            _rolePermissionReadFromWrite = rolePermissionReadFromWrite;
            _rolePermissionWrite = rolePermissionWrite;
            _uow = uow;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            foreach (var item in SeedConstants.SeedCatalog.RolePermissions)
            {
                var roleExists = await _roleReadFromWrite.AnyAsync(x => x.Id == item.RoleId);

                if (!roleExists)
                {
                    throw new InvalidOperationException(
                        $"Cannot seed RolePermission because RoleId '{item.RoleId}' does not exist.");
                }

                var permissionExists = await _permissionReadFromWrite.AnyAsync(x => x.Id == item.PermissionId);

                if (!permissionExists)
                {
                    throw new InvalidOperationException(
                        $"Cannot seed RolePermission because PermissionId '{item.PermissionId}' does not exist.");
                }

                var rolePermissionExists = await _rolePermissionReadFromWrite.AnyAsync(
                    x => x.RoleId == item.RoleId && x.PermissionId == item.PermissionId);

                if (rolePermissionExists)
                {
                    _logger.LogInformation(
                        "RolePermission RoleId {RoleId}, PermissionId {PermissionId} already exists. Skipping.",
                        item.RoleId,
                        item.PermissionId);

                    continue;
                }

                var rolePermission = RolePermission.Create(
                    roleId: item.RoleId,
                    permissionId: item.PermissionId,
                    createdByApplicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId);

                await _rolePermissionWrite.AddAsync(rolePermission);

                _logger.LogInformation(
                    "RolePermission RoleId {RoleId}, PermissionId {PermissionId} seeded successfully.",
                    item.RoleId,
                    item.PermissionId);
            }

            await _uow.SaveChangesAsync();
        }
    }
}