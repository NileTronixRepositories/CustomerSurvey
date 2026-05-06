using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class Permission : AggregateRoot<Guid>
    {
        private readonly List<RolePermission> _rolePermissions = new();

        public string Name { get; private set; } = string.Empty;
        public bool IsSystemPermission { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        private Permission()
        {
        }

        private Permission(Guid id)
            : base(id)
        {
        }

        public static Permission CreateSeeded(
            Guid id,
            string name,
            Guid createdByApplicationUserId)
        {
            return new Permission(id)
            {
                Name = name.Trim(),
                IsSystemPermission = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}