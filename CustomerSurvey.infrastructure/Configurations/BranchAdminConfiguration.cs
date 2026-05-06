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
    internal sealed class BranchAdminConfiguration : IEntityTypeConfiguration<BranchAdmin>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<BranchAdmin> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();

            builder.HasIndex(x => x.BranchId);

            builder.HasOne(x => x.ApplicationUser)
                .WithOne()
                .HasForeignKey<BranchAdmin>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}