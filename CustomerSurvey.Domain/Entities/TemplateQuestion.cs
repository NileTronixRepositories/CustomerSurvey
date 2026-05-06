using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class TemplateQuestion : Entity<Guid>
    {
        public Guid TemplateId { get; private set; }
        public Template Template { get; private set; } = null!;

        public Guid QuestionId { get; private set; }
        public Question Question { get; private set; } = null!;

        public int Order { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private TemplateQuestion()
        {
        }

        private TemplateQuestion(Guid id)
            : base(id)
        {
        }

        public static TemplateQuestion Create(
            Guid templateId,
            Guid questionId,
            int order,
            Guid createdByApplicationUserId)
        {
            return new TemplateQuestion(Guid.NewGuid())
            {
                TemplateId = templateId,
                QuestionId = questionId,
                Order = order,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void ChangeOrder(int order)
        {
            Order = order;
        }
    }
}