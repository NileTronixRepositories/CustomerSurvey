using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignGlobalAnonymousTemplateToBranch;

internal sealed class GetGlobalAnonymousTemplateConditionsForAssignmentSpec
    : Specification<AnonymousTemplateQuestionCondition>
{
    public GetGlobalAnonymousTemplateConditionsForAssignmentSpec(Guid templateId)
    {
        AddCriteria(x => x.AnonymousTemplateId == templateId && x.IsActive);
        AddOrderBy(x => x.Order);
    }
}
