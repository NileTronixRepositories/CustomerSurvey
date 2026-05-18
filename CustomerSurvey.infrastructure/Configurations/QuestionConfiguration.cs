using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class QuestionConfiguration
        : IEntityTypeConfiguration<Question>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BranchId)
                .IsRequired(false);

            builder.Property(x => x.GroupId)
                .IsRequired();
            builder.Property(x => x.Scope)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(QuestionScope.Branch);

            builder.Property(x => x.TextEn)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.TextAr)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => x.GroupId);

            builder.HasIndex(x => x.Scope);

            builder.HasIndex(x => new
            {
                x.Scope,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.BranchId,
                x.IsActive
            });

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("Question", table =>
            {
                table.HasCheckConstraint(
                    "CK_Question_Scope_BranchId",
                    "([Scope] = 1 AND [BranchId] IS NOT NULL) OR ([Scope] = 2 AND [BranchId] IS NULL)");
            });
        }
    }
}