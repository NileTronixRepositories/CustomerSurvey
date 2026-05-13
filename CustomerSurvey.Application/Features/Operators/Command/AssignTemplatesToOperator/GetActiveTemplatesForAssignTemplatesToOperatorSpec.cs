using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    internal sealed record ActiveTemplateForAssignTemplatesToOperatorDto
    {
        public Guid TemplateId { get; init; }
    }

    internal sealed class GetActiveTemplatesForAssignTemplatesToOperatorSpec
        : Specification<Template, ActiveTemplateForAssignTemplatesToOperatorDto>
    {
        public GetActiveTemplatesForAssignTemplatesToOperatorSpec(
            IReadOnlyCollection<Guid> templateIds)
        {
            AddCriteria(x =>
                templateIds.Contains(x.Id) &&
                x.IsActive);

            Select(x => new ActiveTemplateForAssignTemplatesToOperatorDto
            {
                TemplateId = x.Id
            });
        }
    }
}