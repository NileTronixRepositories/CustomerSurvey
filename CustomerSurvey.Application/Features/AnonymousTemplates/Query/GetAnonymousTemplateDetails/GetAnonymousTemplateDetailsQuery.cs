using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    public sealed record GetAnonymousTemplateDetailsQuery
        : IQuery<GetAnonymousTemplateDetailsResponse>
    {
        public Guid AnonymousTemplateId { get; init; }
    }
}