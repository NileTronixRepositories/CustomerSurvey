using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    internal sealed class GetAnonymousTemplateBasicDetailsSpec
        : Specification<AnonymousTemplate, GetAnonymousTemplateDetailsResponse>
    {
        public GetAnonymousTemplateBasicDetailsSpec(
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

            Select(x => new GetAnonymousTemplateDetailsResponse
            {
                AnonymousTemplateId = x.Id,

                BranchId = x.BranchId,

                BranchNameEn = x.Branch == null
                    ? null
                    : x.Branch.NameEn,

                BranchNameAr = x.Branch == null
                    ? null
                    : x.Branch.NameAr,

                Scope = x.Scope,
                ScopeName = x.Scope.ToString(),
                IsGlobal = x.Scope == AnonymousTemplateScope.Global,

                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Description = x.Description,

                ActiveFrom = x.ActiveFrom,
                ExpireTo = x.ExpireTo,

                IsActive = x.IsActive,
                IsArchived = x.IsArchived,
                LogoPath = x.LogoPath,

                PublicUrl = x.PublicUrl,
                QrCode = x.QrCode,

                CreatedByApplicationUserId = x.CreatedByApplicationUserId,
                CreatedOnUtc = x.CreatedOnUtc,
                ModifiedOnUtc = x.ModifiedOnUtc
            });
        }
    }
}
