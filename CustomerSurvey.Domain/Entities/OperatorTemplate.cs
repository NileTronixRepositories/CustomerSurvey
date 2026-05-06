using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class OperatorTemplate : Entity<Guid>
    {
        public Guid OperatorId { get; private set; }
        public Operator Operator { get; private set; } = null!;

        public Guid TemplateId { get; private set; }
        public Template Template { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private OperatorTemplate()
        {
        }

        private OperatorTemplate(Guid id)
            : base(id)
        {
        }

        public static OperatorTemplate Create(
            Guid operatorId,
            Guid templateId,
            Guid createdByApplicationUserId)
        {
            return new OperatorTemplate(Guid.NewGuid())
            {
                OperatorId = operatorId,
                TemplateId = templateId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}