using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousTemplateQuestionConditionConfiguration
        : IEntityTypeConfiguration<AnonymousTemplateQuestionCondition>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousTemplateQuestionCondition> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousTemplateId)
                .IsRequired();

            builder.Property(x => x.ParentAnonymousTemplateQuestionId)
                .IsRequired();

            builder.Property(x => x.ChildAnonymousTemplateQuestionId)
                .IsRequired();

            builder.Property(x => x.TriggerType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.SelectedQuestionOptionId)
                .IsRequired(false);

            builder.Property(x => x.TriggerValue)
                .IsRequired(false);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.AnonymousTemplateId);

            builder.HasIndex(x => x.ParentAnonymousTemplateQuestionId);

            builder.HasIndex(x => x.ChildAnonymousTemplateQuestionId);

            builder.HasIndex(x => x.SelectedQuestionOptionId);

            builder.HasIndex(x => new
            {
                x.AnonymousTemplateId,
                x.ParentAnonymousTemplateQuestionId,
                x.ChildAnonymousTemplateQuestionId,
                x.TriggerType,
                x.SelectedQuestionOptionId,
                x.TriggerValue
            })
            .IsUnique();

            builder.HasOne(x => x.AnonymousTemplate)
                .WithMany()
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ParentAnonymousTemplateQuestion)
                .WithMany()
                .HasForeignKey(x => x.ParentAnonymousTemplateQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChildAnonymousTemplateQuestion)
                .WithMany()
                .HasForeignKey(x => x.ChildAnonymousTemplateQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SelectedQuestionOption)
                .WithMany()
                .HasForeignKey(x => x.SelectedQuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("AnonymousTemplateQuestionCondition", table =>
            {
                table.HasCheckConstraint(
                    "CK_AnonymousTemplateQuestionCondition_Order_Positive",
                    "[Order] > 0");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateQuestionCondition_Not_Same_Question",
                    "[ParentAnonymousTemplateQuestionId] <> [ChildAnonymousTemplateQuestionId]");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateQuestionCondition_SingleChoice",
                    $"([TriggerType] <> {(int)QuestionConditionTriggerType.SingleChoiceOption}) OR " +
                    "([SelectedQuestionOptionId] IS NOT NULL AND [TriggerValue] IS NULL)");

                table.HasCheckConstraint(
                    "CK_AnonymousTemplateQuestionCondition_RatingOrSmile",
                    $"([TriggerType] NOT IN ({(int)QuestionConditionTriggerType.StarRatingValue}, {(int)QuestionConditionTriggerType.SmileValue})) OR " +
                    "([SelectedQuestionOptionId] IS NULL AND [TriggerValue] BETWEEN 1 AND 5)");
            });
        }
    }
}