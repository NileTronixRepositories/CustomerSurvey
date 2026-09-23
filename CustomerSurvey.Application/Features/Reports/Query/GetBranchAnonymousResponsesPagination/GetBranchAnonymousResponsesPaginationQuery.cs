using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;

public sealed class GetBranchAnonymousResponsesPaginationQuery
    : SearchParameters, IQuery<Pagination<BranchAnonymousResponsePaginationItemResponse>>
{
    public Guid? AnonymousTemplateId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public decimal? MinScorePercentage { get; init; }

    public decimal? MaxScorePercentage { get; init; }

    public bool? HasComplaint { get; init; }

    public bool? HasVoice { get; init; }

    public CustomerSurvey.Application.Features.Reports.Shared.SatisfactionCategory? SatisfactionCategory { get; init; }

    public bool? IsScored { get; init; }

    public Guid? QuestionId { get; init; }

    public string? CustomInputName { get; init; }

    public CustomerSurvey.Domain.Enums.TemplateCustomInputType? CustomInputType { get; init; }

    public string? CustomInputValue { get; init; }
}
