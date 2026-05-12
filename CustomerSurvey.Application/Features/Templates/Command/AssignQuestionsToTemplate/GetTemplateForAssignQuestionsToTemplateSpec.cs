using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    internal sealed record TemplateForAssignQuestionsToTemplateDto
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public bool IsActive { get; init; }

        public TemplateStatus Status { get; init; }
    }

    internal sealed class GetTemplateForAssignQuestionsToTemplateSpec
        : Specification<Template, TemplateForAssignQuestionsToTemplateDto>
    {
        public GetTemplateForAssignQuestionsToTemplateSpec(
            Guid templateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == templateId &&
                x.BranchId == branchId);

            Select(x => new TemplateForAssignQuestionsToTemplateDto
            {
                TemplateId = x.Id,
                BranchId = x.BranchId,
                IsActive = x.IsActive,
                Status = x.Status
            });
        }
    }
}