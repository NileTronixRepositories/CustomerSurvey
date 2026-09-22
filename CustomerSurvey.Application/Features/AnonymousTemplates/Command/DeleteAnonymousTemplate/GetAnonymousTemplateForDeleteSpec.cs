using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    internal sealed class GetAnonymousTemplateForDeleteSpec
        : Specification<AnonymousTemplate>
    {
        public GetAnonymousTemplateForDeleteSpec(
            Guid anonymousTemplateId,
            bool isSuperAdmin,
            Guid? currentBranchId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            if (isSuperAdmin)
            {
                AddCriteria(x =>
                    x.Scope == AnonymousTemplateScope.Global ||
                    (x.Scope == AnonymousTemplateScope.Branch &&
                     x.SourceGlobalAnonymousTemplateId.HasValue));
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
