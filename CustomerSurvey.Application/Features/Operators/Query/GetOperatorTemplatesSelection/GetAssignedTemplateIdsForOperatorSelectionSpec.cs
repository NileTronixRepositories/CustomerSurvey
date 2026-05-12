using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    internal sealed record AssignedTemplateIdForOperatorSelectionDto
    {
        public Guid TemplateId { get; init; }
    }

    internal sealed class GetAssignedTemplateIdsForOperatorSelectionSpec
        : Specification<OperatorTemplate, AssignedTemplateIdForOperatorSelectionDto>
    {
        public GetAssignedTemplateIdsForOperatorSelectionSpec(Guid operatorId)
        {
            AddCriteria(x => x.OperatorId == operatorId);

            Select(x => new AssignedTemplateIdForOperatorSelectionDto
            {
                TemplateId = x.TemplateId
            });
        }
    }
}