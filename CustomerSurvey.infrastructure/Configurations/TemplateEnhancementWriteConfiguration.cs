using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations;

internal sealed class TemplateEnhancementWriteConfiguration
    : IEntityTypeConfiguration<Template>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.Property(x => x.LogoPath)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.TemplateFamilyId).IsRequired(false);
        builder.Property(x => x.OriginTemplateId).IsRequired(false);

        builder.HasIndex(x => x.BranchId);

        builder.HasIndex(x => new { x.BranchId, x.OriginTemplateId })
            .IsUnique()
            .HasDatabaseName("UX_Template_Branch_OriginTemplateId")
            .HasFilter("[OriginTemplateId] IS NOT NULL");

        builder.HasIndex(x => new { x.BranchId, x.TemplateFamilyId });
    }
}

internal sealed class AnonymousTemplateEnhancementWriteConfiguration
    : IEntityTypeConfiguration<AnonymousTemplate>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<AnonymousTemplate> builder)
    {
        builder.Property(x => x.PublicUrl).IsRequired(false);
        builder.Property(x => x.IsArchived).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LogoPath).IsRequired(false).HasMaxLength(500);
        builder.Property(x => x.TemplateFamilyId).IsRequired(false);
        builder.Property(x => x.SourceGlobalAnonymousTemplateId).IsRequired(false);
        builder.Property(x => x.OriginAnonymousTemplateId).IsRequired(false);

        builder.HasIndex(x => x.BranchId);

        builder.HasIndex(x => new { x.BranchId, x.SourceGlobalAnonymousTemplateId })
            .IsUnique()
            .HasDatabaseName("UX_AnonymousTemplate_Branch_GlobalSource")
            .HasFilter("[SourceGlobalAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");

        builder.HasIndex(x => new { x.BranchId, x.OriginAnonymousTemplateId })
            .IsUnique()
            .HasDatabaseName("UX_AnonymousTemplate_Branch_OriginAnonymousTemplateId")
            .HasFilter("[OriginAnonymousTemplateId] IS NOT NULL AND [BranchId] IS NOT NULL");

        builder.HasIndex(x => new { x.BranchId, x.TemplateFamilyId });
    }
}
