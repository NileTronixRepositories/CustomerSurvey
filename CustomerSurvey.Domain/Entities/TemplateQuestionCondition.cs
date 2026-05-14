using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class TemplateQuestionCondition : Entity<Guid>
    {
        public Guid TemplateId { get; private set; }

        public Template Template { get; private set; } = null!;

        public Guid ParentTemplateQuestionId { get; private set; }

        public TemplateQuestion ParentTemplateQuestion { get; private set; } = null!;

        public Guid ChildTemplateQuestionId { get; private set; }

        public TemplateQuestion ChildTemplateQuestion { get; private set; } = null!;

        public QuestionConditionTriggerType TriggerType { get; private set; }

        public Guid? SelectedQuestionOptionId { get; private set; }

        public QuestionOption? SelectedQuestionOption { get; private set; }

        public int? TriggerValue { get; private set; }

        public int Order { get; private set; }

        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private TemplateQuestionCondition()
        {
        }

        public static TemplateQuestionCondition CreateForSingleChoice(
            Guid templateId,
            Guid parentTemplateQuestionId,
            Guid childTemplateQuestionId,
            Guid selectedQuestionOptionId,
            int order,
            Guid createdByApplicationUserId)
        {
            return new TemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                TemplateId = templateId,
                ParentTemplateQuestionId = parentTemplateQuestionId,
                ChildTemplateQuestionId = childTemplateQuestionId,
                TriggerType = QuestionConditionTriggerType.SingleChoiceOption,
                SelectedQuestionOptionId = selectedQuestionOptionId,
                TriggerValue = null,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static TemplateQuestionCondition CreateForStarRating(
            Guid templateId,
            Guid parentTemplateQuestionId,
            Guid childTemplateQuestionId,
            int starRatingValue,
            int order,
            Guid createdByApplicationUserId)
        {
            return new TemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                TemplateId = templateId,
                ParentTemplateQuestionId = parentTemplateQuestionId,
                ChildTemplateQuestionId = childTemplateQuestionId,
                TriggerType = QuestionConditionTriggerType.StarRatingValue,
                SelectedQuestionOptionId = null,
                TriggerValue = starRatingValue,
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static TemplateQuestionCondition CreateForSmiles(
            Guid templateId,
            Guid parentTemplateQuestionId,
            Guid childTemplateQuestionId,
            int smileValue,
            int order,
            Guid createdByApplicationUserId)
        {
            return new TemplateQuestionCondition
            {
                Id = Guid.NewGuid(),
                TemplateId = templateId,
                ParentTemplateQuestionId = parentTemplateQuestionId,
                ChildTemplateQuestionId = childTemplateQuestionId,
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