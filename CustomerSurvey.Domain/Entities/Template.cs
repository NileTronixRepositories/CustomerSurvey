using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Template : AggregateRoot<Guid>
    {
        private readonly List<TemplateQuestion> _templateQuestions = new();
        private readonly List<TemplateCustomInput> _customInputs = new();

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string? Description { get; private set; }

        public DateTime ActiveFrom { get; private set; }
        public DateTime? ExpireTo { get; private set; }

        public TemplateStatus Status { get; private set; }
        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<TemplateQuestion> TemplateQuestions =>
            _templateQuestions.AsReadOnly();

        public IReadOnlyCollection<TemplateCustomInput> CustomInputs =>
            _customInputs.AsReadOnly();

        private Template()
        {
        }

        public static Template Create(
            Guid branchId,
            string nameEn,
            string? nameAr,
            string? description,
            DateTime activeFrom,
            DateTime? expireTo,
            Guid createdByApplicationUserId)
        {
            return new Template
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                ActiveFrom = activeFrom,
                ExpireTo = expireTo,
                Status = TemplateStatus.Active,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string nameEn,
            string? nameAr,
            string? description,
            DateTime activeFrom,
            DateTime? expireTo)
        {
            NameEn = nameEn.Trim();
            NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            ActiveFrom = activeFrom;
            ExpireTo = expireTo;
        }

        public bool IsCurrentlyAvailable(DateTime utcNow)
        {
            return IsActive &&
                   ActiveFrom <= utcNow &&
                   (!ExpireTo.HasValue || ExpireTo.Value > utcNow);
        }

        public void AddCustomInput(TemplateCustomInput customInput)
        {
            if (_customInputs.Any(x => x.Name == customInput.Name && x.IsActive))
            {
                return;
            }

            _customInputs.Add(customInput);
        }

        public void Activate()
        {
            if (!IsActive)
            {
                return;
            }

            Status = TemplateStatus.Active;
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            Status = TemplateStatus.Inactive;
        }

        public void ReturnToDraft()
        {
            if (!IsActive)
            {
                return;
            }

            Status = TemplateStatus.Draft;
        }

        public void Restore()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;
            Status = TemplateStatus.Draft;
        }
    }
}