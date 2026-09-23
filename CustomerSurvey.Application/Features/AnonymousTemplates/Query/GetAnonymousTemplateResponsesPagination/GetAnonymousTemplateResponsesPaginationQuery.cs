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

        public CustomerSurvey.Application.Features.Reports.Shared.SatisfactionCategory? SatisfactionCategory { get; init; }

        public bool? IsScored { get; init; }

        public Guid? QuestionId { get; init; }

        public string? CustomInputName { get; init; }

        public CustomerSurvey.Domain.Enums.TemplateCustomInputType? CustomInputType { get; init; }

        public string? CustomInputValue { get; init; }
    }
}
