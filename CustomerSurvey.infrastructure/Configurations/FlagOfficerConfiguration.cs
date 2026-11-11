using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations;

internal sealed class FlagOfficerConfiguration
    : IEntityTypeConfiguration<FlagOfficer>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<FlagOfficer> builder)
    {
        builder.ToTable("FlagOfficers");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.ApplicationUserId).IsUnique();
        builder.HasOne(x => x.ApplicationUser).WithOne()
            .HasForeignKey<FlagOfficer>(x => x.ApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany()
            .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApplicationUser>().WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
