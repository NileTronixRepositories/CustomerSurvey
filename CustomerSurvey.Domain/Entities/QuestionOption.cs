using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class QuestionOption : Entity<Guid>
    {
        public Guid QuestionId { get; private set; }

        public Question Question { get; private set; } = null!;

        public string TextEn { get; private set; } = string.Empty;

        public string? TextAr { get; private set; }

        public int Order { get; private set; }

        // SingleChoice scores range from 1 to 5. Dropdown options use a neutral value of 0.
        public int Value { get; private set; }

        public bool IsActive { get; private set; } = true;
        public Guid? OriginQuestionOptionId { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private QuestionOption()
        {
        }

        public static QuestionOption Create(
           Guid questionId,
           string textEn,
           string? textAr,
           int order,
           int value,
           Guid createdByApplicationUserId,
           Guid? originQuestionOptionId = null)
        {
            return new QuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                TextEn = textEn.Trim(),
                TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim(),
                Order = order,
                Value = value,
                IsActive = true,
                OriginQuestionOptionId = originQuestionOptionId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string textEn,
            string? textAr,
            int order,
            int value)
        {
            TextEn = textEn.Trim();
            TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim();
            ChangeOrder(order);
            Value = value;
        }

        public void ChangeOrder(int order)
        {
            Order = order;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Restore()
        {
            IsActive = true;
        }
    }
}
