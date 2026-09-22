using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Question : AggregateRoot<Guid>
    {
        private readonly List<QuestionOption> _options = new();

        public Guid? BranchId { get; private set; }
        public Branch? Branch { get; private set; }

        public Guid GroupId { get; private set; }
        public QuestionGroup Group { get; private set; } = null!;

        public QuestionScope Scope { get; private set; }

        public string TextEn { get; private set; } = string.Empty;
        public string? TextAr { get; private set; }

        public QuestionType Type { get; private set; }

        public bool IsActive { get; private set; }
        public Guid? OriginQuestionId { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<QuestionOption> Options => _options.AsReadOnly();

        public bool IsGlobal => Scope == QuestionScope.Global;

        public bool IsBranchScoped => Scope == QuestionScope.Branch;

        private Question()
        {
        }

        private Question(Guid id)
            : base(id)
        {
        }

        // Old endpoint compatible factory.
        // Existing POST /api/questions will keep using this.
        public static Question Create(
            Guid branchId,
            Guid groupId,
            string textEn,
            string? textAr,
            QuestionType type,
            Guid createdByApplicationUserId,
            Guid? originQuestionId = null)
        {
            return new Question(Guid.NewGuid())
            {
                BranchId = branchId,
                GroupId = groupId,
                Scope = QuestionScope.Branch,
                TextEn = textEn.Trim(),
                TextAr = string.IsNullOrWhiteSpace(textAr) ? null : textAr.Trim(),
                Type = type,
                IsActive = true,
                OriginQuestionId = originQuestionId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        // Future SuperAdmin Global Question factory.
        // No endpoint will use it now until we explicitly start Global endpoints.
        public static Question CreateGlobal(
            Guid groupId,
            string textEn,
            string? textAr,
            QuestionType type,
            Guid createdByApplicationUserId)
        {
            return new Question(Guid.NewGuid())
            {
                BranchId = null,
                GroupId = groupId,
                Scope = QuestionScope.Global,
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
