using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
    {
        public void Configure(EntityTypeBuilder<Template> builder)
        {
            builder.HasKey(x => x.Id);

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

            builder.Property(x => x.LogoPath)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.TemplateFamilyId)
                .IsRequired(false);

            builder.Property(x => x.OriginTemplateId)
                .IsRequired(false);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.BranchId);

            builder.HasIndex(x => new { x.BranchId, x.NameEn })
                .IsUnique();

            builder.HasIndex(x => new { x.BranchId, x.OriginTemplateId })
                .IsUnique()
                .HasDatabaseName("UX_Template_Branch_OriginTemplateId")
                .HasFilter("[OriginTemplateId] IS NOT NULL");

            builder.HasIndex(x => new { x.BranchId, x.TemplateFamilyId });

            builder.HasIndex(x => new { x.IsActive, x.ActiveFrom });

            builder.HasIndex(x => new { x.IsActive, x.ExpireTo });

            builder.HasOne(x => x.Branch)
                .WithMany(x => x.Templates)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.TemplateQuestions)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CustomInputs)
    .WithOne(x => x.Template)
    .HasForeignKey(x => x.TemplateId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
