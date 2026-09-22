using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateAsAnonymous;

internal sealed class GetTemplateConditionsForCopyAsAnonymousSpec
    : Specification<TemplateQuestionCondition>
{
    public GetTemplateConditionsForCopyAsAnonymousSpec(Guid templateId)
    {
        AddCriteria(x => x.TemplateId == templateId && x.IsActive);
        AddOrderBy(x => x.Order);
    }
}
