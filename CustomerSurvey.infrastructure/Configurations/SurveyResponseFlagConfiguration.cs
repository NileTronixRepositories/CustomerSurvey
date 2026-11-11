using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CustomerSurvey.infrastructure.Configurations;

internal sealed class SurveyResponseFlagConfiguration
    : IEntityTypeConfiguration<SurveyResponseFlag>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<SurveyResponseFlag> builder)
    {
        builder.ToTable("SurveyResponseFlags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(x => x.SurveyResponseId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(x => x.CreatedByApplicationUserId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(x => x.CreatedOnUtc).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(x => x.Brief).HasMaxLength(SurveyResponseFlag.MaxBriefLength);
        builder.Property(x => x.Answer).HasMaxLength(SurveyResponseFlag.MaxAnswerLength);
        builder.HasIndex(x => new { x.SurveyResponseId, x.Type });
        builder.HasIndex(x => new { x.Type, x.SurveyResponseId });
        builder.HasOne(x => x.SurveyResponse).WithMany(x => x.Flags)
            .HasForeignKey(x => x.SurveyResponseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.CreatedBy).WithMany()
            .HasForeignKey(x => x.CreatedByApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.BriefUpdatedBy).WithMany()
            .HasForeignKey(x => x.BriefUpdatedByApplicationUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AnsweredBy).WithMany()
            .HasForeignKey(x => x.AnsweredByApplicationUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
