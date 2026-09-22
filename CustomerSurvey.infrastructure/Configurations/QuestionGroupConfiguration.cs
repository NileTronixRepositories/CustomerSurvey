using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class QuestionGroupConfiguration
        : IEntityTypeConfiguration<QuestionGroup>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<QuestionGroup> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BranchId)
                .IsRequired(false);

            builder.Property(x => x.Scope)
           .IsRequired()
           .HasConversion<int>()
           .HasDefaultValue(QuestionScope.Branch);

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.OriginQuestionGroupId)
                .IsRequired(false);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => new
            {
                x.BranchId,
                x.IsActive
            });

            builder.HasIndex(x => new
            {
                x.Scope,
                x.IsActive
            });

            builder.HasIndex(x => new { x.BranchId, x.OriginQuestionGroupId })
                .IsUnique()
                .HasDatabaseName("UX_QuestionGroup_Branch_OriginQuestionGroupId")
                .HasFilter("[OriginQuestionGroupId] IS NOT NULL AND [BranchId] IS NOT NULL");

            // Branch groups:
            // Same NameEn is unique only inside same Branch.
            builder.HasIndex(x => new
            {
                x.BranchId,
                x.NameEn
            })
            .IsUnique()
            .HasDatabaseName("UX_QuestionGroup_Branch_NameEn")
            .HasFilter("[Scope] = 1 AND [BranchId] IS NOT NULL");

            // Global groups:
            // Same NameEn is unique globally.
            builder.HasIndex(x => x.NameEn)
                .IsUnique()
                .HasDatabaseName("UX_QuestionGroup_Global_NameEn")
                .HasFilter("[Scope] = 2 AND [BranchId] IS NULL");

            builder.HasOne(x => x.Branch)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Questions)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("QuestionGroup", table =>
            {
                table.HasCheckConstraint(
                    "CK_QuestionGroup_Scope_BranchId",
                    "([Scope] = 1 AND [BranchId] IS NOT NULL) OR ([Scope] = 2 AND [BranchId] IS NULL)");
            });
        }
    }
}
