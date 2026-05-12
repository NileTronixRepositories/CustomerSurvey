using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    internal sealed record TemplateForOperatorSelectionDto
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;
    }

    internal sealed class GetTemplatesForOperatorSelectionSpec
        : Specification<Template, TemplateForOperatorSelectionDto>
    {
        public GetTemplatesForOperatorSelectionSpec(string? searchText)
        {
            AddCriteria(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var value = searchText.Trim();

                AddCriteria(x =>
                    x.NameEn.Contains(value) ||
                    (x.NameAr != null && x.NameAr.Contains(value)) ||
                    (x.Description != null && x.Description.Contains(value)) ||
                    x.Branch.NameEn.Contains(value) ||
                    (x.Branch.NameAr != null && x.Branch.NameAr.Contains(value)) ||
                    x.Branch.Code.Contains(value));
            }

            AddOrderBy(x => x.NameEn);

            Select(x => new TemplateForOperatorSelectionDto
            {
                TemplateId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                BranchId = x.BranchId,
                BranchNameEn = x.Branch.NameEn,
                BranchNameAr = x.Branch.NameAr,
                BranchCode = x.Branch.Code
            });
        }
    }
}