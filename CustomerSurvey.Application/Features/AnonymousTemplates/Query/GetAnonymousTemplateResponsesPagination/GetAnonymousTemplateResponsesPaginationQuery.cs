using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    public sealed class GetAnonymousTemplateResponsesPaginationQuery
        : SearchParameters, IQuery<Pagination<AnonymousTemplateResponsePaginationItemResponse>>
    {
        public Guid AnonymousTemplateId { get; init; }

        public DateTime? FromDate { get; init; }

        public DateTime? ToDate { get; init; }

        public decimal? MinScorePercentage { get; init; }

        public decimal? MaxScorePercentage { get; init; }
    }
}