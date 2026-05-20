using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousSurveyResponseCustomInputValueConfiguration
        : IEntityTypeConfiguration<AnonymousSurveyResponseCustomInputValue>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousSurveyResponseCustomInputValue> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousSurveyResponseId)
                .IsRequired();

            builder.Property(x => x.AnonymousTemplateCustomInputId)
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

            builder.HasIndex(x => x.AnonymousSurveyResponseId);

            builder.HasIndex(x => x.AnonymousTemplateCustomInputId);

            builder.HasIndex(x => new
            {
                x.AnonymousSurveyResponseId,
                x.AnonymousTemplateCustomInputId
            })
            .IsUnique();

            builder.HasOne(x => x.AnonymousSurveyResponse)
                .WithMany(x => x.CustomInputValues)
                .HasForeignKey(x => x.AnonymousSurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AnonymousTemplateCustomInput)
                .WithMany()
                .HasForeignKey(x => x.AnonymousTemplateCustomInputId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("AnonymousSurveyResponseCustomInputValue", table =>
            {
                table.HasCheckConstraint(
                    "CK_ASRCIV_String_Value",
                    $"([TypeSnapshot] <> {(int)TemplateCustomInputType.String}) OR " +
                    "([StringValue] IS NOT NULL AND [IntegerValue] IS NULL)");

                table.HasCheckConstraint(
                    "CK_ASRCIV_Integer_Value",
                    $"([TypeSnapshot] <> {(int)TemplateCustomInputType.Integer}) OR " +
                    "([IntegerValue] IS NOT NULL AND [StringValue] IS NULL)");
            });
        }
    }
}