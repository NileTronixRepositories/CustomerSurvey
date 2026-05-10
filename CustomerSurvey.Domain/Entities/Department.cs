using BuildingBlock.Domain.EntitiesHelper;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Domain.Entities
{
    public sealed class Department : AggregateRoot<Guid>
    {
        private readonly List<DepartmentAdmin> _departmentAdmins = new();
        private readonly List<Operator> _operators = new();

        public string NameEn { get; private set; } = string.Empty;

        public string? NameAr { get; private set; }

        public bool IsActive { get; private set; } = true;

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<DepartmentAdmin> DepartmentAdmins => _departmentAdmins;

        public IReadOnlyCollection<Operator> Operators => _operators;

        private Department()
        {
        }

        public static Department Create(
            string nameEn,
            string? nameAr,
            Guid createdByApplicationUserId)
        {
            return new Department
            {
                Id = Guid.NewGuid(),
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