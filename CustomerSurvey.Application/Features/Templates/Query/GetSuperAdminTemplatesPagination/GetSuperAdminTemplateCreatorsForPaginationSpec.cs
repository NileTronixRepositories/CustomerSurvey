using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

internal sealed class GetSuperAdminTemplateCreatorsForPaginationSpec
    : Specification<ApplicationUser, SuperAdminTemplatePaginationCreatorDto>
{
    public GetSuperAdminTemplateCreatorsForPaginationSpec(
        IReadOnlyCollection<Guid> applicationUserIds)
    {
        AddCriteria(x => applicationUserIds.Contains(x.Id));

        Select(x => new SuperAdminTemplatePaginationCreatorDto
        {
            ApplicationUserId = x.Id,
            NameEn = x.NameEn,
            NameAr = x.NameAr
        });
    }
}
