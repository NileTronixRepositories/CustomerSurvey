using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    internal sealed class GetPublicAnonymousTemplateBasicSpec
        : Specification<AnonymousTemplate, PublicAnonymousTemplateBasicDto>
    {
        public GetPublicAnonymousTemplateBasicSpec(Guid anonymousTemplateId)
        {
            AddCriteria(x => x.Id == anonymousTemplateId);

            Select(x => new PublicAnonymousTemplateBasicDto
            {
                AnonymousTemplateId = x.Id,
                BranchId = x.BranchId,
                Scope = x.Scope,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,
                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo,
                IsActive = x.IsActive,
                LogoPath = x.LogoPath,
                BranchNameEn = x.Branch == null ? null : x.Branch.NameEn,
                BranchNameAr = x.Branch == null ? null : x.Branch.NameAr
            });
        }
    }
}
