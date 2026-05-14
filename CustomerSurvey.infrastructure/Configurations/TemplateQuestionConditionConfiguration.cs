using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class TemplateQuestionConditionConfiguration
        : IEntityTypeConfiguration<TemplateQuestionCondition>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<TemplateQuestionCondition> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TemplateId)
                .IsRequired();

            builder.Property(x => x.ParentTemplateQuestionId)
                .IsRequired();

            builder.Property(x => x.ChildTemplateQuestionId)
                .IsRequired();

            builder.Property(x => x.TriggerType)
                .IsRequired();

            builder.Property(x => x.SelectedQuestionOptionId)
                .IsRequired(false);

            builder.Property(x => x.TriggerValue)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.TemplateId);

            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.ParentTemplateQuestionId
            });

            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.ChildTemplateQuestionId
            });

            builder.HasIndex(x => x.ParentTemplateQuestionId);

            builder.HasIndex(x => x.ChildTemplateQuestionId);

            builder.HasIndex(x => x.SelectedQuestionOptionId);

            // SingleChoice condition:
            // SelectedQuestionOptionId is required.
            // TriggerValue must be null.
            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.ParentTemplateQuestionId,
                x.ChildTemplateQuestionId,
                x.TriggerType,
                x.SelectedQuestionOptionId
            })
            .IsUnique()
            .HasDatabaseName("UX_TQC_SingleChoice")
            .HasFilter(
                $"[TriggerType] = {(int)QuestionConditionTriggerType.SingleChoiceOption} " +
                "AND [SelectedQuestionOptionId] IS NOT NULL " +
                "AND [TriggerValue] IS NULL " +
                "AND [IsActive] = 1");

            // StarRating / Smile condition:
            // TriggerValue is required.
            // SelectedQuestionOptionId must be null.
            builder.HasIndex(x => new
            {
                x.TemplateId,
                x.ParentTemplateQuestionId,
                x.ChildTemplateQuestionId,
                x.TriggerType,
                x.TriggerValue
            })
            .IsUnique()
            .HasDatabaseName("UX_TQC_ValueTrigger")
            .HasFilter(
                $"[TriggerType] IN ({(int)QuestionConditionTriggerType.StarRatingValue}, {(int)QuestionConditionTriggerType.SmileValue}) " +
                "AND [SelectedQuestionOptionId] IS NULL " +
                "AND [TriggerValue] IS NOT NULL " +
                "AND [IsActive] = 1");

            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentTemplateQuestion)
                .WithMany()
                .HasForeignKey(x => x.ParentTemplateQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChildTemplateQuestion)
                .WithMany()
                .HasForeignKey(x => x.ChildTemplateQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SelectedQuestionOption)
                .WithMany()
                .HasForeignKey(x => x.SelectedQuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}