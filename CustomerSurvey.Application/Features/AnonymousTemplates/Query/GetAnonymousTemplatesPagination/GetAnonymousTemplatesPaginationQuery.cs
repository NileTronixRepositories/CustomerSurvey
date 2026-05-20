using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    public sealed class GetAnonymousTemplatesPaginationQuery
          : SearchParameters, IQuery<Pagination<AnonymousTemplatePaginationItemResponse>>
    {
        public AnonymousTemplateScope? Scope { get; init; }

        public Guid? BranchId { get; init; }

        public bool? IsActive { get; init; }
    }
}