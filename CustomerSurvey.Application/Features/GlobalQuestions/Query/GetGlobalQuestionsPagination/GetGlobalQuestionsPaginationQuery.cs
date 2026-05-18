using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination
{
    public sealed class GetGlobalQuestionsPaginationQuery
       : SearchParameters, IQuery<Pagination<GlobalQuestionPaginationItemResponse>>
    {
        public bool? IsActive { get; init; }

        public Guid? GroupId { get; init; }
    }
}