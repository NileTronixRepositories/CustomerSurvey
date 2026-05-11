using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    public sealed class GetQuestionGroupsPaginationQuery
          : SearchParameters, IQuery<Pagination<QuestionGroupPaginationItemResponse>>
    {
        public bool? IsActive { get; init; }
    }
}