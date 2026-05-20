using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousSurveyAnswerConfiguration
        : IEntityTypeConfiguration<AnonymousSurveyAnswer>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousSurveyAnswer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousSurveyResponseId)
                .IsRequired();

            builder.Property(x => x.AnonymousTemplateQuestionId)
                .IsRequired();

            builder.Property(x => x.QuestionId)
                .IsRequired();

            builder.Property(x => x.QuestionType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.SelectedQuestionOptionId)
                .IsRequired(false);

            builder.Property(x => x.StarRatingValue)
                .IsRequired(false);

            builder.Property(x => x.SmileValue)
                .IsRequired(false);

            builder.Property(x => x.TextAnswer)
                .IsRequired(false)
                .HasMaxLength(4000);

            builder.Property(x => x.VoiceFileName)
                .IsRequired(false)
                .HasMaxLength(300);

            builder.HasIndex(x => new
            {
                x.AnonymousSurveyResponseId,
                x.AnonymousTemplateQuestionId
            })
            .IsUnique();

            builder.HasIndex(x => x.QuestionId);

            builder.HasIndex(x => x.SelectedQuestionOptionId);

            builder.HasOne(x => x.AnonymousSurveyResponse)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.AnonymousSurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AnonymousTemplateQuestion)
                .WithMany()
                .HasForeignKey(x => x.AnonymousTemplateQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SelectedQuestionOption)
                .WithMany()
                .HasForeignKey(x => x.SelectedQuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}