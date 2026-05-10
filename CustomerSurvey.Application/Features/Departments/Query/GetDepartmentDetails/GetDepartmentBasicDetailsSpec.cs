using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    internal sealed record DepartmentBasicDetailsDto
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    internal sealed class GetDepartmentBasicDetailsSpec
        : Specification<Department, DepartmentBasicDetailsDto>
    {
        public GetDepartmentBasicDetailsSpec(Guid departmentId)
        {
            AddCriteria(x => x.Id == departmentId);

            Select(x => new DepartmentBasicDetailsDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}