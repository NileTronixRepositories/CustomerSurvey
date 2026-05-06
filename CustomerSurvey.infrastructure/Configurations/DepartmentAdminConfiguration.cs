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
    internal sealed class DepartmentAdminConfiguration : IEntityTypeConfiguration<DepartmentAdmin>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<DepartmentAdmin> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();

            builder.HasIndex(x => x.DepartmentId);

            builder.HasOne(x => x.ApplicationUser)
                .WithOne()
                .HasForeignKey<DepartmentAdmin>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.DepartmentAdmins)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}