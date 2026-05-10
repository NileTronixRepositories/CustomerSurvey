using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record BranchBasicDetailsDto
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;

        public string? Address { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    internal sealed class GetBranchBasicDetailsSpec
        : Specification<Branch, BranchBasicDetailsDto>
    {
        public GetBranchBasicDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.Id == branchId);

            Select(x => new BranchBasicDetailsDto
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Code = x.Code,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc
            });
        }
    }
}