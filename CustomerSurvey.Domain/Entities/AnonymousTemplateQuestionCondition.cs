using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousTemplateQuestionCondition : Entity<Guid>
    {
        public Guid AnonymousTemplateId { get; private set; }
        public AnonymousTemplate AnonymousTemplate { get; private set; } = null!;

        public Guid ParentAnonymousTemplateQuestionId { get; private set; }
        public AnonymousTemplateQuestion ParentAnonymousTemplateQuestion { get; private set; } = null!;

        public Guid ChildAnonymousTemplateQuestionId { get; private set; }
        public AnonymousTemplateQuestion ChildAnonymousTemplateQuestion { get; private set; } = null!;

        public QuestionConditionTriggerType TriggerType { get; private set; }

        public Guid? SelectedQuestionOptionId { get; private set; }
        public QuestionOption? SelectedQuestionOption { get; private set; }

        public int? TriggerValue { get; private set; }

        public int Order { get; private set; }

        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private AnonymousTemplateQuestionCondition()
        {
        }

        public static AnonymousTemplateQuestionCondition CreateForSingleChoice(
            Guid anonymousTemplateId,
            Guid parentAnonymousTemplateQuestionId,
            Guid childAnonymousTemplateQuestionId,
            Guid selectedQuestionOptionId,
            int order,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                AnonymousTemplateId = anonymousTemplateId,
                ParentAnonymousTemplateQuestionId = parentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = childAnonymousTemplateQuestionId,
                TriggerType = QuestionConditionTriggerType.SingleChoiceOption,
                SelectedQuestionOptionId = selectedQuestionOptionId,
                TriggerValue = null,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static AnonymousTemplateQuestionCondition CreateForStarRating(
            Guid anonymousTemplateId,
            Guid parentAnonymousTemplateQuestionId,
            Guid childAnonymousTemplateQuestionId,
            int starRatingValue,
            int order,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                AnonymousTemplateId = anonymousTemplateId,
                ParentAnonymousTemplateQuestionId = parentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = childAnonymousTemplateQuestionId,
                TriggerType = QuestionConditionTriggerType.StarRatingValue,
                SelectedQuestionOptionId = null,
                TriggerValue = starRatingValue,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static AnonymousTemplateQuestionCondition CreateForSmiles(
            Guid anonymousTemplateId,
            Guid parentAnonymousTemplateQuestionId,
            Guid childAnonymousTemplateQuestionId,
            int smileValue,
            int order,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                AnonymousTemplateId = anonymousTemplateId,
                ParentAnonymousTemplateQuestionId = parentAnonymousTemplateQuestionId,
                ChildAnonymousTemplateQuestionId = childAnonymousTemplateQuestionId,
                TriggerType = QuestionConditionTriggerType.SmileValue,
                SelectedQuestionOptionId = null,
                TriggerValue = smileValue,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}