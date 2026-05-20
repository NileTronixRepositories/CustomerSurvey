using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousTemplateCustomInputConfiguration
        : IEntityTypeConfiguration<AnonymousTemplateCustomInput>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousTemplateCustomInput> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousTemplateId)
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LabelEn)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.LabelAr)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.IsRequired)
                .IsRequired();

            builder.Property(x => x.MinLength)
                .IsRequired(false);

            builder.Property(x => x.MaxLength)
                .IsRequired(false);

            builder.Property(x => x.MinValue)
                .IsRequired(false);

            builder.Property(x => x.MaxValue)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.AnonymousTemplateId);

            builder.HasIndex(x => new { x.AnonymousTemplateId, x.Name })
                .IsUnique();

            builder.HasIndex(x => new { x.AnonymousTemplateId, x.IsActive, x.Order });

            builder.HasOne(x => x.AnonymousTemplate)
                .WithMany(x => x.CustomInputs)
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("AnonymousTemplateCustomInput", table =>
            {
                table.HasCheckConstraint(
                    "CK_AnonymousTemplateCustomInput_Order_Positive",
                    "[Order] > 0");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateCustomInput_String_Validation",
                    $"([Type] <> {(int)TemplateCustomInputType.String}) OR " +
                    "([MinValue] IS NULL AND [MaxValue] IS NULL)");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateCustomInput_Integer_Validation",
                    $"([Type] <> {(int)TemplateCustomInputType.Integer}) OR " +
                    "([MinLength] IS NULL AND [MaxLength] IS NULL)");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateCustomInput_Length_Range",
                    "([MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength])");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateCustomInput_Value_Range",
                    "([MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue])");
            });
        }
    }
}