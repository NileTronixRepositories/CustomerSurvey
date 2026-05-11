using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    public sealed class GetTemplatesPaginationQuery
        : SearchParameters, IQuery<Pagination<TemplatePaginationItemResponse>>
    {
        public bool? IsActive { get; init; }
    }
}