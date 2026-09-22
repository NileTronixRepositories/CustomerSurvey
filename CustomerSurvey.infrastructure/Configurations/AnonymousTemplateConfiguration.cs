using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousTemplateConfiguration : IEntityTypeConfiguration<AnonymousTemplate>
    {
        public void Configure(EntityTypeBuilder<AnonymousTemplate> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BranchId)
                .IsRequired(false);

            builder.Property(x => x.Scope)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.ActiveFrom)
                .IsRequired();

            builder.Property(x => x.ExpireTo)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.IsArchived)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.PublicUrl)
                .IsRequired(false)
                .HasMaxLength(1000);

            builder.Property(x => x.QrCode)
                .IsRequired(false);

            builder.Property(x => x.LogoPath)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.TemplateFamilyId)
                .IsRequired(false);

            builder.Property(x => x.SourceGlobalAnonymousTemplateId)
                .IsRequired(false);

            builder.Property(x => x.OriginAnonymousTemplateId)
                .IsRequired(false);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.Scope);

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => new { x.IsActive, x.ActiveFrom });

            builder.HasIndex(x => new { x.IsActive, x.ExpireTo });

            builder.HasIndex(x => new { x.BranchId, x.NameEn })
                .IsUnique()
                .HasFilter("[Scope] = 1 AND [BranchId] IS NOT NULL");

            builder.HasIndex(x => new { x.BranchId, x.SourceGlobalAnonymousTemplateId })
                .IsUnique()
                .HasDatabaseName("UX_AnonymousTemplate_Branch_GlobalSource")
                .HasFilter("[SourceGlobalAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");

            builder.HasIndex(x => new { x.BranchId, x.OriginAnonymousTemplateId })
                .IsUnique()
                .HasDatabaseName("UX_AnonymousTemplate_Branch_OriginAnonymousTemplateId")
                .HasFilter("[OriginAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");

            builder.HasIndex(x => new { x.BranchId, x.TemplateFamilyId });

            builder.HasIndex(x => x.NameEn)
                .IsUnique()
                .HasDatabaseName("IX_AnonymousTemplate_Global_NameEn")
                .HasFilter("[Scope] = 2");

            builder.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Questions)
                .WithOne(x => x.AnonymousTemplate)
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CustomInputs)
                .WithOne(x => x.AnonymousTemplate)
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("AnonymousTemplate", table =>
            {
                table.HasCheckConstraint(
                    "CK_AnonymousTemplate_Scope_BranchId",
                    "([Scope] = 1 AND [BranchId] IS NOT NULL) OR ([Scope] = 2 AND [BranchId] IS NULL)");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplate_ExpireTo_After_ActiveFrom",
                    "[ExpireTo] IS NULL OR [ExpireTo] > [ActiveFrom]");
            });
        }
    }
}
