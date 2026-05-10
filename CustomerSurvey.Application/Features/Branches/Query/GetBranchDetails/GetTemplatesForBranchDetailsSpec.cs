using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    internal sealed record TemplateForBranchDetailsDto
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public TemplateStatus Status { get; init; }

        public int QuestionsCount { get; init; }
    }

    internal sealed class GetTemplatesForBranchDetailsSpec
        : Specification<Template, TemplateForBranchDetailsDto>
    {
        public GetTemplatesForBranchDetailsSpec(Guid branchId)
        {
            AddCriteria(x => x.BranchId == branchId);

            AddOrderBy(x => x.NameEn);

            Select(x => new TemplateForBranchDetailsDto
            {
                TemplateId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                Status = x.Status,
                QuestionsCount = x.TemplateQuestions.Count
            });
        }
    }
}