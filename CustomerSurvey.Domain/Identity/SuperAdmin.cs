using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class SuperAdmin : AggregateRoot<Guid>
    {
        public Guid ApplicationUserId { get; private set; }
        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private SuperAdmin()
        {
        }

        private SuperAdmin(Guid id)
            : base(id)
        {
        }

        public static SuperAdmin CreateSeeded(
            Guid id,
            Guid applicationUserId)
        {
            return new SuperAdmin(id)
            {
                ApplicationUserId = applicationUserId,
                CreatedByApplicationUserId = applicationUserId
            };
        }
    }
}