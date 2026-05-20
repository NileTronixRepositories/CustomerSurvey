using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateForResponsesPaginationSpec
        : Specification<AnonymousTemplate, AnonymousTemplateForResponsesPaginationDto>
    {
        public GetAnonymousTemplateForResponsesPaginationSpec(
            Guid anonymousTemplateId,
            bool isSuperAdmin,
            Guid? currentBranchId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            if (!isSuperAdmin)
            {
                AddCriteria(x =>
                    x.Scope == AnonymousTemplateScope.Branch &&
                    x.BranchId == currentBranchId);
            }

            Select(x => new AnonymousTemplateForResponsesPaginationDto
            {
                AnonymousTemplateId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                IsActive = x.IsActive
            });
        }
    }
}