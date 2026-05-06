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
    internal sealed class SuperAdminConfiguration : IEntityTypeConfiguration<SuperAdmin>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<SuperAdmin> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();

            builder.HasOne(x => x.ApplicationUser)
                .WithOne()
                .HasForeignKey<SuperAdmin>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}