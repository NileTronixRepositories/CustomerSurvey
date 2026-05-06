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
    internal sealed class SuperAdminRoleSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 5;

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadFromWrite;
        private readonly IWriteReadRepository<Role> _roleReadFromWrite;
        private readonly IWriteReadRepository<UserRole> _userRoleReadFromWrite;
        private readonly IWriteRepository<UserRole> _userRoleWrite;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SuperAdminRoleSeeder> _logger;

        public SuperAdminRoleSeeder(
            IWriteReadRepository<ApplicationUser> applicationUserReadFromWrite,
            IWriteReadRepository<Role> roleReadFromWrite,
            IWriteReadRepository<UserRole> userRoleReadFromWrite,
            IWriteRepository<UserRole> userRoleWrite,
            IUnitOfWork uow,
            ILogger<SuperAdminRoleSeeder> logger)
        {
            _applicationUserReadFromWrite = applicationUserReadFromWrite;
            _roleReadFromWrite = roleReadFromWrite;
            _userRoleReadFromWrite = userRoleReadFromWrite;
            _userRoleWrite = userRoleWrite;
            _uow = uow;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var superAdminUserExists = await _applicationUserReadFromWrite.AnyAsync(
                x => x.Id == SeedConstants.SuperAdminSeed.ApplicationUserId);

            if (!superAdminUserExists)
            {
                throw new InvalidOperationException(
                    "Cannot assign System Administrator role because seeded Super Admin user does not exist.");
            }

            var systemAdministratorRoleExists = await _roleReadFromWrite.AnyAsync(
                x => x.Id == SeedConstants.SeedIds.Roles.SystemAdministrator);

            if (!systemAdministratorRoleExists)
            {
                throw new InvalidOperationException(
                    "Cannot assign System Administrator role because seeded role does not exist.");
            }

            var userRoleExists = await _userRoleReadFromWrite.AnyAsync(
                x => x.ApplicationUserId == SeedConstants.SuperAdminSeed.ApplicationUserId
                     && x.RoleId == SeedConstants.SeedIds.Roles.SystemAdministrator);

            if (userRoleExists)
            {
                _logger.LogInformation("System Administrator role already assigned to Super Admin. Skipping.");
                return;
            }

            var userRole = UserRole.Create(
                applicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId,
                roleId: SeedConstants.SeedIds.Roles.SystemAdministrator,
                createdByApplicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId);

            await _userRoleWrite.AddAsync(userRole);

            await _uow.SaveChangesAsync();

            _logger.LogInformation("System Administrator role assigned to Super Admin successfully.");
        }
    }
}