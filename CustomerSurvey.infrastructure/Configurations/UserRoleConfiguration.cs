using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => x.RoleId);

            builder.HasIndex(x => new { x.ApplicationUserId, x.RoleId })
                .IsUnique();
        }
    }
}