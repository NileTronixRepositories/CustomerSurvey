using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class AnonymousTemplate : AggregateRoot<Guid>
    {
        private readonly List<AnonymousTemplateQuestion> _questions = new();
        private readonly List<AnonymousTemplateCustomInput> _customInputs = new();

        public Guid? BranchId { get; private set; }
        public Branch? Branch { get; private set; }

        public AnonymousTemplateScope Scope { get; private set; }

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string? Description { get; private set; }

        public DateTime ActiveFrom { get; private set; }
        public DateTime? ExpireTo { get; private set; }

        public TemplateStatus Status { get; private set; }
        public bool IsActive { get; private set; }

        public string PublicUrl { get; private set; } = string.Empty;
        public string? QrCode { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<AnonymousTemplateQuestion> Questions =>
            _questions.AsReadOnly();

        public IReadOnlyCollection<AnonymousTemplateCustomInput> CustomInputs =>
            _customInputs.AsReadOnly();

        public bool IsGlobal => Scope == AnonymousTemplateScope.Global;

        public bool IsBranchScoped => Scope == AnonymousTemplateScope.Branch;

        private AnonymousTemplate()
        {
        }

        public static AnonymousTemplate CreateBranchTemplate(
            Guid branchId,
            string nameEn,
            string? nameAr,
            string? description,
            DateTime activeFrom,
            DateTime? expireTo,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplate
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Scope = AnonymousTemplateScope.Branch,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                ActiveFrom = activeFrom,
                ExpireTo = expireTo,
                Status = TemplateStatus.Draft,
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public static AnonymousTemplate CreateGlobalTemplate(
            string nameEn,
            string? nameAr,
            string? description,
            DateTime activeFrom,
            DateTime? expireTo,
            Guid createdByApplicationUserId)
        {
            return new AnonymousTemplate
            {
                Id = Guid.NewGuid(),
                BranchId = null,
                Scope = AnonymousTemplateScope.Global,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                ActiveFrom = activeFrom,
                ExpireTo = expireTo,
                Status = TemplateStatus.Draft,
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

        public void SetPublicAccess(string publicUrl, string? qrCode)
        {
            PublicUrl = publicUrl.Trim();
            QrCode = string.IsNullOrWhiteSpace(qrCode) ? null : qrCode.Trim();
        }

        public bool IsCurrentlyAvailable(DateTime utcNow)
        {
            return IsActive &&
                   ActiveFrom <= utcNow &&
                   (!ExpireTo.HasValue || ExpireTo.Value > utcNow);
        }

        public void AddCustomInput(AnonymousTemplateCustomInput customInput)
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

        public void ReturnToDraft()
        {
            if (!IsActive)
            {
                return;
            }

            Status = TemplateStatus.Draft;
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