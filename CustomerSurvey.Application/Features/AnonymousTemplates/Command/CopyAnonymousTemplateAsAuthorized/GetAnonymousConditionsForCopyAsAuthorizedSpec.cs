using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CopyAnonymousTemplateAsAuthorized;

internal sealed class GetAnonymousConditionsForCopyAsAuthorizedSpec
    : Specification<AnonymousTemplateQuestionCondition>
{
    public GetAnonymousConditionsForCopyAsAuthorizedSpec(Guid templateId)
    {
        AddCriteria(x => x.AnonymousTemplateId == templateId && x.IsActive);
        AddOrderBy(x => x.Order);
    }
}
