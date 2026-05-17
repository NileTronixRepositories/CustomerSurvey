using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

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
            IReadOnlyCollection<Guid> templateIds,
            DateTime utcNow)
        {
            AddCriteria(x =>
                templateIds.Contains(x.Id) &&
                x.IsActive &&
                x.ActiveFrom <= utcNow &&
                (!x.ExpireTo.HasValue || x.ExpireTo.Value > utcNow));

            Select(x => new ActiveTemplateForAssignTemplatesToOperatorDto
            {
                TemplateId = x.Id
            });
        }
    }
}