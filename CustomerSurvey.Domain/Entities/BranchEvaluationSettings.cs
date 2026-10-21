using BuildingBlock.Domain.EntitiesHelper;

namespace CustomerSurvey.Domain.Entities;

public sealed class BranchEvaluationSettings : Entity<Guid>
{
    public Guid BranchId { get; private set; }
    public decimal NeutralMinimum { get; private set; } = 60m;
    public decimal SatisfiedMinimum { get; private set; } = 80m;
    public decimal MediumRiskMinimum { get; private set; } = 50m;
    public decimal HealthyMinimum { get; private set; } = 75m;
    public DateTime CreatedOnUtc { get; private set; }
    public DateTime UpdatedOnUtc { get; private set; }
    public Guid UpdatedByApplicationUserId { get; private set; }

    public static BranchEvaluationSettings Create(Guid branchId, Guid actorId)
        => new() { Id = Guid.NewGuid(), BranchId = branchId, CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow, UpdatedByApplicationUserId = actorId };

    public static bool IsValid(decimal neutral, decimal satisfied, decimal mediumRisk, decimal healthy)
        => neutral >= 0 && neutral < satisfied && satisfied <= 100 &&
           mediumRisk >= 0 && mediumRisk < healthy && healthy <= 100;

    public void Update(decimal neutral, decimal satisfied, decimal mediumRisk, decimal healthy, Guid actorId)
    {
        if (!IsValid(neutral, satisfied, mediumRisk, healthy))
            throw new ArgumentException("Each lower threshold must be below its upper threshold, within 0..100.");
        NeutralMinimum = neutral;
        SatisfiedMinimum = satisfied;
        MediumRiskMinimum = mediumRisk;
        HealthyMinimum = healthy;
        UpdatedOnUtc = DateTime.UtcNow;
        UpdatedByApplicationUserId = actorId;
    }
}
