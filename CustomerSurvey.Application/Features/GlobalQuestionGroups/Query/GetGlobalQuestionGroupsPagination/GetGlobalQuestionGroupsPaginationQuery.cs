using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    public sealed class GetGlobalQuestionGroupsPaginationQuery
       : SearchParameters, IQuery<Pagination<GlobalQuestionGroupPaginationItemResponse>>
    {
        public bool? IsActive { get; init; }
    }
}