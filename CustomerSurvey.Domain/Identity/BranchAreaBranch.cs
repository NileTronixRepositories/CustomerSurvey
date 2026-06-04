using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class BranchAreaBranch : AggregateRoot<Guid>
    {
        public Guid BranchAreaId { get; private set; }
        public BranchArea BranchArea { get; private set; } = null!;

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private BranchAreaBranch()
        {
        }

        private BranchAreaBranch(Guid id)
            : base(id)
        {
        }

        public static BranchAreaBranch Create(
            Guid branchAreaId,
            Guid branchId,
            Guid createdByApplicationUserId)
        {
            return new BranchAreaBranch(Guid.NewGuid())
            {
                BranchAreaId = branchAreaId,
                BranchId = branchId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}
