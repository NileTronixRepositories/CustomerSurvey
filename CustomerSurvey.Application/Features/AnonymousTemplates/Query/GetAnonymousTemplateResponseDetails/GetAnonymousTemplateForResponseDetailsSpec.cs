using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed class GetAnonymousTemplateForResponseDetailsSpec
        : Specification<AnonymousTemplate, AnonymousTemplateForResponseDetailsDto>
    {
        public GetAnonymousTemplateForResponseDetailsSpec(
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

            Select(x => new AnonymousTemplateForResponseDetailsDto
            {
                AnonymousTemplateId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                IsActive = x.IsActive
            });
        }
    }
}