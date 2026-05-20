namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed record CurrentBranchActorForSurveyResponseDetailsDto
{
    public Guid BranchId { get; init; }
}