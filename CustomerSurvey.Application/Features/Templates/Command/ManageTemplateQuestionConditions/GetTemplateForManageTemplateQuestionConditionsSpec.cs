using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    internal sealed record TemplateForManageTemplateQuestionConditionsDto
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public bool IsActive { get; init; }

    }

    internal sealed class GetTemplateForManageTemplateQuestionConditionsSpec
        : Specification<Template, TemplateForManageTemplateQuestionConditionsDto>
    {
        public GetTemplateForManageTemplateQuestionConditionsSpec(
            Guid templateId,
            Guid branchId)
        {
            AddCriteria(x =>
                x.Id == templateId &&
                x.BranchId == branchId);

            Select(x => new TemplateForManageTemplateQuestionConditionsDto
            {
                TemplateId = x.Id,
                BranchId = x.BranchId,
                IsActive = x.IsActive
            });
        }
    }
}
