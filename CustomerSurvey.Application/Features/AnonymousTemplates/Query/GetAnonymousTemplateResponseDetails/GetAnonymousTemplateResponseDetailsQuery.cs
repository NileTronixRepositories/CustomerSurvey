using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    public sealed record GetAnonymousTemplateResponseDetailsQuery
        : IQuery<GetAnonymousTemplateResponseDetailsResponse>
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid ResponseId { get; init; }
    }
}