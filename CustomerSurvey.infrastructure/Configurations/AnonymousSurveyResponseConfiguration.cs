using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousSurveyResponseConfiguration
        : IEntityTypeConfiguration<AnonymousSurveyResponse>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousSurveyResponse> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousTemplateId)
                .IsRequired();

            builder.Property(x => x.SubmittedOnUtc)
                .IsRequired();

            builder.Property(x => x.ActualScore)
                .IsRequired();

            builder.Property(x => x.MaxScore)
                .IsRequired();

            builder.Property(x => x.ScorePercentage)
                .IsRequired()
                .HasPrecision(5, 2);

            builder.HasIndex(x => x.AnonymousTemplateId);

            builder.HasIndex(x => x.SubmittedOnUtc);

            builder.HasIndex(x => new { x.AnonymousTemplateId, x.SubmittedOnUtc });

            builder.HasOne(x => x.AnonymousTemplate)
                .WithMany()
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Answers)
                .WithOne(x => x.AnonymousSurveyResponse)
                .HasForeignKey(x => x.AnonymousSurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CustomInputValues)
                .WithOne(x => x.AnonymousSurveyResponse)
                .HasForeignKey(x => x.AnonymousSurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}