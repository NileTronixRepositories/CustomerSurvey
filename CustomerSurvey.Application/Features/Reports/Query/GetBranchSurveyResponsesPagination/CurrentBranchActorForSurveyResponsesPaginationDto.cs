namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

internal sealed record CurrentBranchActorForSurveyResponsesPaginationDto
{
    public Guid BranchId { get; init; }
}