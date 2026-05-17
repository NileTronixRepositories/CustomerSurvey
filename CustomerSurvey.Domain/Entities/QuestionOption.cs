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

        // New: used for SingleChoice scoring only.
        // Allowed business range: 1 to 5.
        public int Value { get; private set; }

        public bool IsActive { get; private set; } = true;

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
           Guid createdByApplicationUserId)
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
            Order = order;
            Value = value;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}