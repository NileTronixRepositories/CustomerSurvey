using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Identity
{
    public sealed class Operator : AggregateRoot<Guid>
    {
        private readonly List<OperatorTemplate> _operatorTemplates = new();

        public Guid ApplicationUserId { get; private set; }
        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid DepartmentId { get; private set; }
        public Department Department { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<OperatorTemplate> OperatorTemplates => _operatorTemplates.AsReadOnly();

        private Operator()
        {
        }

        private Operator(Guid id)
            : base(id)
        {
        }

        public static Operator Create(
            Guid applicationUserId,
            Guid departmentId,
            Guid createdByApplicationUserId)
        {
            return new Operator(Guid.NewGuid())
            {
                ApplicationUserId = applicationUserId,
                DepartmentId = departmentId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}