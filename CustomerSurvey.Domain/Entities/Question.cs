using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Question : AggregateRoot<Guid>
    {
        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public Guid GroupId { get; private set; }
        public QuestionGroup Group { get; private set; } = null!;

        public string TextEn { get; private set; } = string.Empty;
        public string? TextAr { get; private set; }
        public QuestionType Type { get; private set; }
        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        private Question()
        {
        }

        private Question(Guid id)
            : base(id)
        {
        }

        public static Question Create(
            Guid branchId,
            Guid groupId,
            string textEn,
            string? textAr,
            QuestionType type,
            Guid createdByApplicationUserId)
        {
            return new Question(Guid.NewGuid())
            {
                BranchId = branchId,
                GroupId = groupId,
                TextEn = textEn.Trim(),
                TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim(),
                Type = type,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
    Guid groupId,
    string textEn,
    string? textAr,
    QuestionType type)
        {
            GroupId = groupId;
            TextEn = textEn.Trim();
            TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim();
            Type = type;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}