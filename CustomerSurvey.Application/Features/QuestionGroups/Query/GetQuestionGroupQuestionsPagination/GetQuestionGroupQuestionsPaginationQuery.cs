using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    public sealed class GetQuestionGroupQuestionsPaginationQuery
        : SearchParameters, IQuery<Pagination<QuestionByGroupPaginationItemResponse>>
    {
        public Guid QuestionGroupId { get; set; }
    }
}
