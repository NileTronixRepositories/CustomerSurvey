using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

internal sealed class GetTemplateQuestionConditionsForCopyToBranchSpec
    : Specification<TemplateQuestionCondition>
{
    public GetTemplateQuestionConditionsForCopyToBranchSpec(Guid templateId)
    {
        AddCriteria(x => x.TemplateId == templateId && x.IsActive);

        AddOrderBy(x => x.Order);
    }
}
