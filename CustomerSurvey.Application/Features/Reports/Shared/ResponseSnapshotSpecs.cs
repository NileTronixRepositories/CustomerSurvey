using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Shared;

internal sealed class GetAuthorizedResponseSnapshotSpec : Specification<SurveyResponse, ResponseSnapshot>
{
    public GetAuthorizedResponseSnapshotSpec(Guid responseId)
    {
        UseNoTracking();
        AddCriteria(x => x.Id == responseId && x.Snapshot != null);
        Select(x => x.Snapshot!);
    }
}

internal sealed class GetAnonymousResponseSnapshotSpec : Specification<AnonymousSurveyResponse, ResponseSnapshot>
{
    public GetAnonymousResponseSnapshotSpec(Guid responseId)
    {
        UseNoTracking();
        AddCriteria(x => x.Id == responseId && x.Snapshot != null);
        Select(x => x.Snapshot!);
    }
}
