using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class TemplateCustomInputConfiguration
       : IEntityTypeConfiguration<TemplateCustomInput>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<TemplateCustomInput> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateId)
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

            builder.Property(x => x.StartWith)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.TemplateId);

            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.Name
            })
            .IsUnique();

            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.IsActive,
                x.Order
            });

            builder.HasIndex(x => x.CreatedByApplicationUserId);

            builder.HasOne(x => x.Template)
                .WithMany(x => x.CustomInputs)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("TemplateCustomInput", table =>
            {
                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_Order_Positive",
                    "[Order] > 0");

                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_String_Validation",
                    $"([Type] <> {(int)TemplateCustomInputType.String}) OR " +
                    "([MinValue] IS NULL AND [MaxValue] IS NULL)");

                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_Integer_Validation",
                    $"([Type] <> {(int)TemplateCustomInputType.Integer}) OR " +
                    "([MinLength] IS NULL AND [MaxLength] IS NULL)");

                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_StartWith_StringOnly",
                    $"([Type] = {(int)TemplateCustomInputType.String}) OR ([StartWith] IS NULL)");

                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_Length_Range",
                    "([MinLength] IS NULL OR [MaxLength] IS NULL OR [MaxLength] >= [MinLength])");

                table.HasCheckConstraint(
                    "CK_TemplateCustomInput_Value_Range",
                    "([MinValue] IS NULL OR [MaxValue] IS NULL OR [MaxValue] >= [MinValue])");
            });
        }
    }
}
