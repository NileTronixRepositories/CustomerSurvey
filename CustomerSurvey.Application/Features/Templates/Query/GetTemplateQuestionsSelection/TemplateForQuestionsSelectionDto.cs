using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed record TemplateForQuestionsSelectionDto
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public TemplateStatus Status { get; init; }

        public bool IsActive { get; init; }
    }

    internal sealed class GetTemplateForQuestionsSelectionSpec
        : Specification<Template, TemplateForQuestionsSelectionDto>
    {
        public GetTemplateForQuestionsSelectionSpec(
            Guid templateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == templateId &&
                x.BranchId == branchId);

            Select(x => new TemplateForQuestionsSelectionDto
            {
                TemplateId = x.Id,
                BranchId = x.BranchId,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Status = x.Status,
                IsActive = x.IsActive
            });
        }
    }
}