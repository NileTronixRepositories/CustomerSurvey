using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed class GetAnonymousTemplateForManageConditionsSpec
        : Specification<AnonymousTemplate>
    {
        public GetAnonymousTemplateForManageConditionsSpec(
            Guid anonymousTemplateId,
            bool isSuperAdmin,
            Guid? currentBranchId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            if (isSuperAdmin)
            {
                AddCriteria(x => x.Scope == AnonymousTemplateScope.Global);
            }
            else
            {
                AddCriteria(x =>
                    x.Scope == AnonymousTemplateScope.Branch &&
                    x.BranchId == currentBranchId);
            }
        }
    }
}