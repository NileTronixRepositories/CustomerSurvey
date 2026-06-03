using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetSuperAdminTemplatesPagination;

public sealed class GetSuperAdminTemplatesPaginationQuery
    : SearchParameters, IQuery<Pagination<SuperAdminTemplatePaginationItemResponse>>
{
    public Guid? BranchId { get; init; }

    public TemplateCatalogKind? TemplateKind { get; init; }

    public bool? IsActive { get; init; }
}
