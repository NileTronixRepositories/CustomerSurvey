using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class QuestionGroup : AggregateRoot<Guid>
    {
        private readonly List<Question> _questions = new();

        public Guid? BranchId { get; private set; }
        public Branch? Branch { get; private set; }

        public QuestionScope Scope { get; private set; }

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }

        public bool IsActive { get; private set; }
        public Guid? OriginQuestionGroupId { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

        public bool IsGlobal => Scope == QuestionScope.Global;

        public bool IsBranchScoped => Scope == QuestionScope.Branch;

        private QuestionGroup()
        {
        }

        private QuestionGroup(Guid id)
            : base(id)
        {
        }

        // Old endpoint compatible factory.
        // Existing POST /api/question-groups will keep using this.
        public static QuestionGroup Create(
            Guid branchId,
            string nameEn,
            string? nameAr,
            Guid createdByApplicationUserId,
            Guid? originQuestionGroupId = null)
        {
            return new QuestionGroup(Guid.NewGuid())
            {
                BranchId = branchId,
                Scope = QuestionScope.Branch,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                IsActive = true,
                OriginQuestionGroupId = originQuestionGroupId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        // Future SuperAdmin Global Question Group factory.
        // No endpoint will use it now until we explicitly start Global endpoints.
        public static QuestionGroup CreateGlobal(
            string nameEn,
            string? nameAr,
            Guid createdByApplicationUserId)
        {
            return new QuestionGroup(Guid.NewGuid())
            {
                BranchId = null,
                Scope = QuestionScope.Global,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                IsActive = true,
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
