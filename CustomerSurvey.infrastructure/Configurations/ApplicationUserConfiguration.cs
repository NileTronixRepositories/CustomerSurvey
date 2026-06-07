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
    internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.PhoneNumber)
                .IsRequired(false)
                .HasMaxLength(30);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.UserType)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.IsFirstLogin)
                .IsRequired();

            builder.Property(x => x.PasswordChangedOnUtc)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.UserName)
                .IsUnique();

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.ApplicationUser)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
