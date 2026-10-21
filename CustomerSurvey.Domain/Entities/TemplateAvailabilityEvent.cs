using BuildingBlock.Domain.EntitiesHelper;

namespace CustomerSurvey.Domain.Entities;

public sealed class TemplateAvailabilityEvent : Entity<Guid>
{
    public Guid TemplateId { get; private set; }
    public int TemplateKind { get; private set; }
    public Guid? BranchId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public bool? PreviousIsActive { get; private set; }
    public DateTime? PreviousActiveFrom { get; private set; }
    public DateTime? PreviousExpireTo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime ActiveFrom { get; private set; }
    public DateTime? ExpireTo { get; private set; }
    public DateTime ChangedOnUtc { get; private set; }
    public Guid? ActorApplicationUserId { get; private set; }
    public bool IsBaseline { get; private set; }

    public static TemplateAvailabilityEvent Create(Guid templateId, int kind, Guid? branchId,
        string eventType, bool active, DateTime activeFrom, DateTime? expireTo, DateTime changedOnUtc,
        Guid? actorId, bool isBaseline = false, bool? previousActive = null,
        DateTime? previousFrom = null, DateTime? previousTo = null)
        => new() { Id = Guid.NewGuid(), TemplateId = templateId, TemplateKind = kind, BranchId = branchId,
            EventType = eventType, IsActive = active, ActiveFrom = activeFrom, ExpireTo = expireTo,
            ChangedOnUtc = changedOnUtc, ActorApplicationUserId = actorId, IsBaseline = isBaseline,
            PreviousIsActive = previousActive, PreviousActiveFrom = previousFrom, PreviousExpireTo = previousTo };
}
