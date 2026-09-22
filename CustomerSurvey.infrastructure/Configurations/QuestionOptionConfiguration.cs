using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class QuestionOptionConfiguration
        : IEntityTypeConfiguration<QuestionOption>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<QuestionOption> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.QuestionId)
                .IsRequired();

            builder.Property(x => x.TextEn)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.TextAr)
                .IsRequired(false)
                .HasMaxLength(300);

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.Value)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.OriginQuestionOptionId)
                .IsRequired(false);

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.QuestionId);

            builder.HasIndex(x => new { x.QuestionId, x.Order })
                .IsUnique();

            builder.HasIndex(x => new { x.QuestionId, x.OriginQuestionOptionId })
                .IsUnique()
                .HasDatabaseName("UX_QuestionOption_Question_OriginQuestionOptionId")
                .HasFilter("[OriginQuestionOptionId] IS NOT NULL");

            builder.HasOne(x => x.Question)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
