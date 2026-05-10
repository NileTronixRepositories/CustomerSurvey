using BuildingBlock.Domain.EntitiesHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Branch : AggregateRoot<Guid>
    {
        private readonly List<Template> _templates = new();
        private readonly List<QuestionGroup> _groups = new();

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public string? Address { get; private set; }
        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<Template> Templates => _templates.AsReadOnly();
        public IReadOnlyCollection<QuestionGroup> Groups => _groups.AsReadOnly();

        private Branch()
        {
        }

        private Branch(Guid id)
            : base(id)
        {
        }

        public static Branch Create(
            string nameEn,
            string? nameAr,
            string code,
            string? address,
            Guid createdByApplicationUserId)
        {
            return new Branch(Guid.NewGuid())
            {
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Code = code.Trim(),
                Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                IsActive = true,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }

        public void Update(
                string nameEn,
                string? nameAr,
                string code,
                string? address)
        {
            NameEn = nameEn.Trim();
            NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim();
            Code = code.Trim();
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
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