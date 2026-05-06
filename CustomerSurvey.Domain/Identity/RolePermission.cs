using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class RolePermission : Entity<Guid>
    {
        public Guid RoleId { get; private set; }
        public Role Role { get; private set; } = null!;

        public Guid PermissionId { get; private set; }
        public Permission Permission { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private RolePermission()
        {
        }

        private RolePermission(Guid id)
            : base(id)
        {
        }

        public static RolePermission Create(
            Guid roleId,
            Guid permissionId,
            Guid createdByApplicationUserId)
        {
            return new RolePermission(Guid.NewGuid())
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}