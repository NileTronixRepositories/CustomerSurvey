using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerSurvey.infrastructure.Configurations;

internal sealed class BranchEvaluationSettingsConfiguration : IEntityTypeConfiguration<BranchEvaluationSettings>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<BranchEvaluationSettings> b)
    {
        b.ToTable("BranchEvaluationSettings", t =>
        {
            t.HasCheckConstraint("CK_BranchEvaluation_Satisfaction", "[NeutralMinimum] >= 0 AND [NeutralMinimum] < [SatisfiedMinimum] AND [SatisfiedMinimum] <= 100");
            t.HasCheckConstraint("CK_BranchEvaluation_Risk", "[MediumRiskMinimum] >= 0 AND [MediumRiskMinimum] < [HealthyMinimum] AND [HealthyMinimum] <= 100");
        });
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.BranchId).IsUnique();
        b.Property(x => x.NeutralMinimum).HasPrecision(5, 2);
        b.Property(x => x.SatisfiedMinimum).HasPrecision(5, 2);
        b.Property(x => x.MediumRiskMinimum).HasPrecision(5, 2);
        b.Property(x => x.HealthyMinimum).HasPrecision(5, 2);
        b.HasOne<Branch>().WithOne(x => x.EvaluationSettings).HasForeignKey<BranchEvaluationSettings>(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ResponseSnapshotConfiguration : IEntityTypeConfiguration<ResponseSnapshot>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<ResponseSnapshot> b)
    {
        b.ToTable("ResponseSnapshots", t => t.HasCheckConstraint("CK_ResponseSnapshot_Source",
            "([TemplateKind] = 1 AND [SurveyResponseId] IS NOT NULL AND [SurveyResponseId] = [ResponseId] AND [AnonymousSurveyResponseId] IS NULL) OR ([TemplateKind] = 2 AND [AnonymousSurveyResponseId] IS NOT NULL AND [AnonymousSurveyResponseId] = [ResponseId] AND [SurveyResponseId] IS NULL)"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Payload).IsRequired().HasColumnType("nvarchar(max)");
        b.Property(x => x.RootScorePercentage).HasPrecision(5, 2);
        b.Property(x => x.LowestScorePercentage).HasPrecision(5, 2);
        b.Property(x => x.AllScorePercentage).HasPrecision(5, 2);
        b.HasIndex(x => new { x.TemplateKind, x.ResponseId }).IsUnique();
        b.HasIndex(x => new { x.TemplateKind, x.TemplateId });
        b.HasOne<SurveyResponse>().WithOne(x => x.Snapshot).HasForeignKey<ResponseSnapshot>(x => x.SurveyResponseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<AnonymousSurveyResponse>().WithOne(x => x.Snapshot).HasForeignKey<ResponseSnapshot>(x => x.AnonymousSurveyResponseId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class TemplateAvailabilityEventConfiguration : IEntityTypeConfiguration<TemplateAvailabilityEvent>, IWriteEntityConfiguration
{
    public void Configure(EntityTypeBuilder<TemplateAvailabilityEvent> b)
    {
        b.ToTable("TemplateAvailabilityEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).IsRequired().HasMaxLength(40);
        b.HasIndex(x => new { x.TemplateKind, x.TemplateId, x.ChangedOnUtc });
        b.HasIndex(x => new { x.TemplateKind, x.TemplateId, x.IsBaseline }).IsUnique().HasFilter("[IsBaseline] = 1");
        b.HasIndex(x => new { x.BranchId, x.ChangedOnUtc });
    }
}
