using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination
{
    internal sealed class GetDepartmentCreatorsForDepartmentsPaginationSpec
        : Specification<ApplicationUser, DepartmentPaginationCreatorDto>
    {
        public GetDepartmentCreatorsForDepartmentsPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.Id));

            Select(x => new DepartmentPaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}