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
            Guid createdByApplicationUserId)
        {
            return new QuestionOption
            {
                Id = Guid.NewGuid(),
                QuestionId = questionId,
                TextEn = textEn.Trim(),
                TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim(),
                Order = order,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string textEn,
            string? textAr,
            int order)
        {
            TextEn = textEn.Trim();
            TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim();
            Order = order;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}