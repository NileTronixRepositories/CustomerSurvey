using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    public sealed record GetPublicAnonymousTemplateQuery
        : IQuery<GetPublicAnonymousTemplateResponse>
    {
        public Guid AnonymousTemplateId { get; init; }
    }
}