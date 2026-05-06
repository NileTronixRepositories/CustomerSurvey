using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Template : AggregateRoot<Guid>
    {
        private readonly List<TemplateQuestion> _templateQuestions = new();

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string? Description { get; private set; }
        public TemplateStatus Status { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<TemplateQuestion> TemplateQuestions => _templateQuestions.AsReadOnly();

        private Template()
        {
        }

        private Template(Guid id)
            : base(id)
        {
        }

        public static Template Create(
            Guid branchId,
            string nameEn,
            string? nameAr,
            string? description,
            Guid createdByApplicationUserId)
        {
            return new Template(Guid.NewGuid())
            {
                BranchId = branchId,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                Status = TemplateStatus.Draft,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
            string nameEn,
            string? nameAr,
            string? description)
        {
            NameEn = nameEn.Trim();
            NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        public void Activate()
        {
            Status = TemplateStatus.Active;
        }

        public void Deactivate()
        {
            Status = TemplateStatus.Inactive;
        }

        public void ReturnToDraft()
        {
            Status = TemplateStatus.Draft;
        }
    }
}