using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class Role : AggregateRoot<Guid>
    {
        private readonly List<UserRole> _userRoles = new();
        private readonly List<RolePermission> _rolePermissions = new();

        public string Name { get; private set; } = string.Empty;
        public bool IsSystemRole { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        private Role()
        {
        }

        private Role(Guid id)
            : base(id)
        {
        }

        public static Role CreateSeeded(
            Guid id,
            string name,
            Guid createdByApplicationUserId)
        {
            return new Role(id)
            {
                Name = name.Trim(),
                IsSystemRole = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}