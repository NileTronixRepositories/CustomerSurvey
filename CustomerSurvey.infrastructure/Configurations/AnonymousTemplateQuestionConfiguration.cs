using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations
{
    internal sealed class AnonymousTemplateQuestionConfiguration
        : IEntityTypeConfiguration<AnonymousTemplateQuestion>, IWriteEntityConfiguration
    {
        public void Configure(EntityTypeBuilder<AnonymousTemplateQuestion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AnonymousTemplateId)
                .IsRequired();

            builder.Property(x => x.QuestionId)
                .IsRequired();

            builder.Property(x => x.Order)
                .IsRequired();

            builder.Property(x => x.CreatedByApplicationUserId)
                .IsRequired();

            builder.HasIndex(x => x.AnonymousTemplateId);

            builder.HasIndex(x => x.QuestionId);

            builder.HasIndex(x => new { x.AnonymousTemplateId, x.QuestionId })
                .IsUnique();

            builder.HasIndex(x => new { x.AnonymousTemplateId, x.Order });

            builder.HasOne(x => x.AnonymousTemplate)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.AnonymousTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Question)
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("AnonymousTemplateQuestion", table =>
            {
                table.HasCheckConstraint(
                    "CK_AnonymousTemplateQuestion_Order_Positive",
                    "[Order] > 0");
            });
        }
    }
}