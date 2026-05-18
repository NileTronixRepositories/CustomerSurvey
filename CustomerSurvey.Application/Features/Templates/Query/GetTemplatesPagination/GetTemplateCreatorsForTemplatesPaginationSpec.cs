using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed class GetTemplateCreatorsForTemplatesPaginationSpec
        : Specification<ApplicationUser, TemplatePaginationCreatorDto>
    {
        public GetTemplateCreatorsForTemplatesPaginationSpec(
            IReadOnlyCollection<Guid> applicationUserIds)
        {
            AddCriteria(x => applicationUserIds.Contains(x.Id));

            Select(x => new TemplatePaginationCreatorDto
            {
                ApplicationUserId = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr
            });
        }
    }
}