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
    internal sealed class TemplateQuestionConfiguration : IEntityTypeConfiguration<TemplateQuestion>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<TemplateQuestion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.TemplateId);

            builder.HasIndex(x => x.QuestionId);

            builder.HasIndex(x => new { x.TemplateId, x.QuestionId })
                .IsUnique();

            builder.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}