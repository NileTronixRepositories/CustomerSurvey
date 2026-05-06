using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Seeding;
using CustomerSurvey.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Seeders
{
    internal sealed class SuperAdminSeeder : ISeeder
    {
        public int ExecutionOrder { get; set; } = 1;

        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadFromWrite;
        private readonly IWriteRepository<ApplicationUser> _applicationUserWrite;

        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadFromWrite;
        private readonly IWriteRepository<SuperAdmin> _superAdminWrite;

        private readonly IUnitOfWork _uow;
        private readonly ILogger<SuperAdminSeeder> _logger;
        private readonly PasswordHasher<ApplicationUser> _passwordHasher;

        public SuperAdminSeeder(
            IWriteReadRepository<ApplicationUser> applicationUserReadFromWrite,
            IWriteRepository<ApplicationUser> applicationUserWrite,
            IWriteReadRepository<SuperAdmin> superAdminReadFromWrite,
            IWriteRepository<SuperAdmin> superAdminWrite,
            IUnitOfWork uow,
            ILogger<SuperAdminSeeder> logger)
        {
            _applicationUserReadFromWrite = applicationUserReadFromWrite;
            _applicationUserWrite = applicationUserWrite;
            _superAdminReadFromWrite = superAdminReadFromWrite;
            _superAdminWrite = superAdminWrite;
            _uow = uow;
            _logger = logger;
            _passwordHasher = new PasswordHasher<ApplicationUser>();
        }

        public async Task SeedAsync()
        {
            await SeedSuperAdminApplicationUserAsync();
            await SeedSuperAdminProfileAsync();

            await _uow.SaveChangesAsync();
        }

        private async Task SeedSuperAdminApplicationUserAsync()
        {
            var existsById = await _applicationUserReadFromWrite.AnyAsync(
                x => x.Id == SeedConstants.SuperAdminSeed.ApplicationUserId);

            if (existsById)
            {
                _logger.LogInformation("Super Admin application user already exists by Id. Skipping.");
                return;
            }

            var existsByUserName = await _applicationUserReadFromWrite.AnyAsync(
                x => x.UserName == SeedConstants.SuperAdminSeed.UserName);

            if (existsByUserName)
            {
                throw new InvalidOperationException(
                    $"Application user '{SeedConstants.SuperAdminSeed.UserName}' already exists with a different Id. Align seed ids before continuing.");
            }

            var existsByEmail = await _applicationUserReadFromWrite.AnyAsync(
                x => x.Email == SeedConstants.SuperAdminSeed.Email);

            if (existsByEmail)
            {
                throw new InvalidOperationException(
                    $"Application user email '{SeedConstants.SuperAdminSeed.Email}' already exists with a different Id. Align seed ids before continuing.");
            }

            var superAdminUser = ApplicationUser.CreateSeededSuperAdmin(
                id: SeedConstants.SuperAdminSeed.ApplicationUserId,
                userName: SeedConstants.SuperAdminSeed.UserName,
                email: SeedConstants.SuperAdminSeed.Email,
                nameEn: SeedConstants.SuperAdminSeed.NameEn);

            var passwordHash = _passwordHasher.HashPassword(
                superAdminUser,
                SeedConstants.SuperAdminSeed.DefaultPassword);

            superAdminUser.SetPasswordHash(passwordHash);

            await _applicationUserWrite.AddAsync(superAdminUser);

            _logger.LogInformation("Super Admin application user seeded successfully.");
        }

        private async Task SeedSuperAdminProfileAsync()
        {
            var existsById = await _superAdminReadFromWrite.AnyAsync(
                x => x.Id == SeedConstants.SuperAdminSeed.SuperAdminId);

            if (existsById)
            {
                _logger.LogInformation("Super Admin profile already exists by Id. Skipping.");
                return;
            }

            var existsByApplicationUserId = await _superAdminReadFromWrite.AnyAsync(
                x => x.ApplicationUserId == SeedConstants.SuperAdminSeed.ApplicationUserId);

            if (existsByApplicationUserId)
            {
                throw new InvalidOperationException(
                    "Super Admin profile already exists with a different Id. Align seed ids before continuing.");
            }

            var superAdmin = SuperAdmin.CreateSeeded(
                id: SeedConstants.SuperAdminSeed.SuperAdminId,
                applicationUserId: SeedConstants.SuperAdminSeed.ApplicationUserId);

            await _superAdminWrite.AddAsync(superAdmin);

            _logger.LogInformation("Super Admin profile seeded successfully.");
        }
    }
}