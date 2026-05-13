using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class SurveyAnswerConfiguration
          : IEntityTypeConfiguration<SurveyAnswer>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<SurveyAnswer> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SurveyResponseId)
                .IsRequired();

            builder.Property(x => x.QuestionId)
                .IsRequired();

            builder.Property(x => x.QuestionType)
                .IsRequired();

            builder.Property(x => x.TextAnswer)
                .IsRequired(false)
                .HasMaxLength(4000);

            builder.Property(x => x.VoiceFileName)
                .IsRequired(false)
                .HasMaxLength(300);

            builder.HasIndex(x => new { x.SurveyResponseId, x.QuestionId })
                .IsUnique();

            builder.HasIndex(x => x.QuestionId);

            builder.HasIndex(x => x.SelectedQuestionOptionId);

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