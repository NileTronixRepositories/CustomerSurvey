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

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public string NameEn { get; private set; } = string.Empty;
        public string? NameAr { get; private set; }
        public string Code { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }

        public Guid CreatedByApplicationUserId { get; private set; }

        public IReadOnlyCollection<DepartmentAdmin> DepartmentAdmins => _departmentAdmins.AsReadOnly();
        public IReadOnlyCollection<Operator> Operators => _operators.AsReadOnly();

        private Department()
        {
        }

        private Department(Guid id)
            : base(id)
        {
        }

        public static Department Create(
            Guid branchId,
            string nameEn,
            string? nameAr,
            string code,
            Guid createdByApplicationUserId)
        {
            return new Department(Guid.NewGuid())
            {
                BranchId = branchId,
                NameEn = nameEn.Trim(),
                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr.Trim(),
                Code = code.Trim(),
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