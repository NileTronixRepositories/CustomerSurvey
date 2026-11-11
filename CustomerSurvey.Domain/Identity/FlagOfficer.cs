using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class FlagOfficer : AggregateRoot<Guid>
    {
        public Guid ApplicationUserId { get; private set; }
        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid DepartmentId { get; private set; }
        public Department Department { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private FlagOfficer()
        {
        }

        private FlagOfficer(Guid id)
            : base(id)
        {
        }

        public static FlagOfficer Create(
            Guid applicationUserId,
            Guid departmentId,
            Guid createdByApplicationUserId)
        {
            return new FlagOfficer(Guid.NewGuid())
            {
                ApplicationUserId = applicationUserId,
                DepartmentId = departmentId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}