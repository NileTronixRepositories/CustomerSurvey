using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class GetAnonymousTemplateQuestionConditionsForCopyToBranchSpec
    : Specification<AnonymousTemplateQuestionCondition>
{
    public GetAnonymousTemplateQuestionConditionsForCopyToBranchSpec(Guid anonymousTemplateId)
    {
        AddCriteria(x => x.AnonymousTemplateId == anonymousTemplateId && x.IsActive);

        AddOrderBy(x => x.Order);
    }
}
