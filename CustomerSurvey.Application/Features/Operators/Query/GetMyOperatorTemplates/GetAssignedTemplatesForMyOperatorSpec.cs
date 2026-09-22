using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record AssignedTemplateForMyOperatorDto
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;
        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }
        public string? LogoPath { get; init; }
    }

    internal sealed class GetAssignedTemplatesForMyOperatorSpec
        : Specification<OperatorTemplate, AssignedTemplateForMyOperatorDto>
    {
        public GetAssignedTemplatesForMyOperatorSpec(Guid operatorId, DateTime utcNow)
        {
            AddCriteria(x =>
      x.OperatorId == operatorId &&
      x.Template.IsActive &&
      x.Template.ActiveFrom <= utcNow &&
      (!x.Template.ExpireTo.HasValue || x.Template.ExpireTo.Value > utcNow));

            AddOrderBy(x => x.Template.NameEn);

            Select(x => new AssignedTemplateForMyOperatorDto
            {
                TemplateId = x.TemplateId,
                NameEn = x.Template.NameEn,
                NameAr = x.Template.NameAr,
                Description = x.Template.Description,
                BranchId = x.Template.BranchId,
                BranchNameEn = x.Template.Branch.NameEn,
                BranchNameAr = x.Template.Branch.NameAr,
                BranchCode = x.Template.Branch.Code,
                ActiveFrom = x.Template.ActiveFrom,
                ExpireTo = x.Template.ExpireTo,
                LogoPath = x.Template.LogoPath,
            });
        }
    }
}
