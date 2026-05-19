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
    internal sealed class SurveyResponseCustomInputValueConfiguration
        : IEntityTypeConfiguration<SurveyResponseCustomInputValue>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<SurveyResponseCustomInputValue> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SurveyResponseId)
                .IsRequired();

            builder.Property(x => x.TemplateCustomInputId)
                .IsRequired();

            builder.Property(x => x.NameSnapshot)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.TypeSnapshot)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.StringValue)
                .IsRequired(false)
                .HasMaxLength(3000);

            builder.Property(x => x.IntegerValue)
                .IsRequired(false);

            builder.HasIndex(x => x.SurveyResponseId);

            builder.HasIndex(x => x.TemplateCustomInputId);

            builder.HasIndex(x => new
            {
                x.SurveyResponseId,
                x.TemplateCustomInputId
            })
            .IsUnique();

            builder.HasOne(x => x.SurveyResponse)
                .WithMany(x => x.CustomInputValues)
                .HasForeignKey(x => x.SurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TemplateCustomInput)
                .WithMany()
                .HasForeignKey(x => x.TemplateCustomInputId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("SurveyResponseCustomInputValue", table =>
            {
                table.HasCheckConstraint(
                    "CK_SRCIV_String_Value",
                    $"([TypeSnapshot] <> {(int)TemplateCustomInputType.String}) OR " +
                    "([StringValue] IS NOT NULL AND [IntegerValue] IS NULL)");

                table.HasCheckConstraint(
                    "CK_SRCIV_Integer_Value",
                    $"([TypeSnapshot] <> {(int)TemplateCustomInputType.Integer}) OR " +
                    "([IntegerValue] IS NOT NULL AND [StringValue] IS NULL)");
            });
        }
    }
}