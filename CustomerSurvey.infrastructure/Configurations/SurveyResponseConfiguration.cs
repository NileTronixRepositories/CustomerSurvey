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
    internal sealed class SurveyResponseConfiguration
         : IEntityTypeConfiguration<SurveyResponse>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<SurveyResponse> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OperatorId)
                .IsRequired();

            builder.Property(x => x.TemplateId)
                .IsRequired();

            builder.Property(x => x.SubmittedOnUtc)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();
            builder.Property(x => x.ActualScore)
    .IsRequired();

            builder.Property(x => x.MaxScore)
                .IsRequired();

            builder.Property(x => x.ScorePercentage)
                .IsRequired()
                .HasPrecision(5, 2);

            builder.HasIndex(x => new { x.TemplateId, x.SubmittedOnUtc });

            builder.HasIndex(x => new { x.OperatorId, x.TemplateId, x.SubmittedOnUtc });
            builder.HasIndex(x => x.OperatorId);

            builder.HasIndex(x => x.TemplateId);

            builder.HasIndex(x => x.SubmittedOnUtc);

            builder.HasOne(x => x.Operator)
                .WithMany()
                .HasForeignKey(x => x.OperatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Template)
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Answers)
                .WithOne(x => x.SurveyResponse)
                .HasForeignKey(x => x.SurveyResponseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.CustomInputValues)
    .WithOne(x => x.SurveyResponse)
    .HasForeignKey(x => x.SurveyResponseId)
    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}