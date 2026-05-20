using BuildingBlock.Domain.EntitiesHelper;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousTemplateQuestion : Entity<Guid>
    {
        public Guid AnonymousTemplateId { get; private set; }
        public AnonymousTemplate AnonymousTemplate { get; private set; } = null!;

        public Guid QuestionId { get; private set; }
        public Question Question { get; private set; } = null!;

        public int Order { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private AnonymousTemplateQuestion()
        {
        }

        public static AnonymousTemplateQuestion Create(
            Guid anonymousTemplateId,
            Guid questionId,
            int order,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplateQuestion
            {
                Id = Guid.NewGuid(),
                AnonymousTemplateId = anonymousTemplateId,
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