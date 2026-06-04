using BuildingBlock.Domain.EntitiesHelper;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class BranchArea : AggregateRoot<Guid>
    {
        private readonly List<BranchAreaBranch> _branches = new();

        public Guid ApplicationUserId { get; private set; }
        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<BranchAreaBranch> Branches => _branches.AsReadOnly();

        private BranchArea()
        {
        }

        private BranchArea(Guid id)
            : base(id)
        {
        }

        public static BranchArea Create(
            Guid applicationUserId,
            Guid createdByApplicationUserId)
        {
            return new BranchArea(Guid.NewGuid())
            {
                ApplicationUserId = applicationUserId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}
