using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class ApplicationUser : AggregateRoot<Guid>
    {
        private readonly List<UserRole> _userRoles = new();

        public string UserName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string PasswordHash { get; private set; } = string.Empty;
        public UserType UserType { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsFirstLogin { get; private set; }
        public DateTime PasswordChangedOnUtc { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private ApplicationUser()
        {
        }

        private ApplicationUser(Guid id)
            : base(id)
        {
        }

        public static ApplicationUser Create(
            string userName,
            string email,
            string nameEn,
            string? nameAr,
            string? phoneNumber,
            string passwordHash,
            UserType userType,
            Guid createdByApplicationUserId)
        {
            return new ApplicationUser(Guid.NewGuid())
            {
                UserName = userName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
                PasswordHash = passwordHash,
                UserType = userType,
                IsActive = true,
                IsFirstLogin = true,
                PasswordChangedOnUtc = DateTime.UtcNow,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static ApplicationUser CreateSeededSuperAdmin(
            Guid id,
            string userName,
            string email,
            string nameEn)
        {
            return new ApplicationUser(id)
            {
                UserName = userName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                NameEn = nameEn.Trim(),
                NameAr = null,
                PhoneNumber = null,
                UserType = UserType.SuperAdmin,
                IsActive = true,
                IsFirstLogin = true,
                PasswordChangedOnUtc = DateTime.UtcNow,
                CreatedByApplicationUserId = id
            };
        }

        public void SetPasswordHash(string passwordHash)
        {
            ResetPassword(passwordHash);
        }

        public void ChangePassword(string passwordHash)
        {
            ChangePassword(passwordHash, DateTime.UtcNow);
        }

        public void ChangePassword(string passwordHash, DateTime changedOnUtc)
        {
            SetPassword(passwordHash, changedOnUtc, isFirstLogin: false);
        }

        public void ResetPassword(string passwordHash)
        {
            ResetPassword(passwordHash, DateTime.UtcNow);
        }

        public void ResetPassword(string passwordHash, DateTime changedOnUtc)
        {
            SetPassword(passwordHash, changedOnUtc, isFirstLogin: true);
        }

        private void SetPassword(
            string passwordHash,
            DateTime changedOnUtc,
            bool isFirstLogin)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
            }

            PasswordHash = passwordHash;
            PasswordChangedOnUtc = changedOnUtc;
            IsFirstLogin = isFirstLogin;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateProfile(
    string nameEn,
    string? nameAr,
    string email,
    string? phoneNumber)
        {
            NameEn = nameEn.Trim();
            NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim();
            Email = email.Trim();
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        }
    }
}
