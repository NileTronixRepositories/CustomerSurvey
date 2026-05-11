using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    public sealed class GetQuestionsPaginationQuery
         : SearchParameters, IQuery<Pagination<QuestionPaginationItemResponse>>
    {
        public bool? IsActive { get; set; }
    }
}