namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed record CurrentBranchActorForQuestionGroupQuestionsPaginationDto
    {
        public Guid BranchId { get; init; }
    }
}
