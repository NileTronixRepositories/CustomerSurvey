using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class BranchAreaBranchConfiguration : IEntityTypeConfiguration<BranchAreaBranch>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<BranchAreaBranch> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BranchAreaId)
                .IsRequired();

            builder.Property(x => x.BranchId)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.BranchAreaId);

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => new { x.BranchAreaId, x.BranchId })
                .IsUnique();

            builder.HasOne(x => x.BranchArea)
                .WithMany(x => x.Branches)
                .HasForeignKey(x => x.BranchAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
