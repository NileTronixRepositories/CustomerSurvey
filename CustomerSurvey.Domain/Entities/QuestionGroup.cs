using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class QuestionGroup : AggregateRoot<Guid>
    {
        private readonly List<Question> _questions = new();

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

        private QuestionGroup()
        {
        }

        private QuestionGroup(Guid id)
            : base(id)
        {
        }

        public static QuestionGroup Create(
            Guid branchId,
            string nameEn,
            string? nameAr,
            Guid createdByApplicationUserId)
        {
            return new QuestionGroup(Guid.NewGuid())
            {
                BranchId = branchId,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string nameEn,
            string? nameAr)
        {
            NameEn = nameEn.Trim();
            NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim();
        }
    }
}