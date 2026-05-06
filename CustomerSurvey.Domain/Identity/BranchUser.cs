using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class BranchUser : AggregateRoot<Guid>
    {
        public Guid ApplicationUserId { get; private set; }
        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private BranchUser()
        {
        }

        private BranchUser(Guid id)
            : base(id)
        {
        }

        public static BranchUser Create(
            Guid applicationUserId,
            Guid branchId,
            Guid createdByApplicationUserId)
        {
            return new BranchUser(Guid.NewGuid())
            {
                ApplicationUserId = applicationUserId,
                BranchId = branchId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}