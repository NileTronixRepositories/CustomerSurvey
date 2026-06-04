using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class BranchAreaConfiguration : IEntityTypeConfiguration<BranchArea>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<BranchArea> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ApplicationUserId)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.ApplicationUserId)
                .IsUnique();

            builder.HasOne(x => x.ApplicationUser)
                .WithOne()
                .HasForeignKey<BranchArea>(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Branches)
                .WithOne(x => x.BranchArea)
                .HasForeignKey(x => x.BranchAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
