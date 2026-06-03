namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed record QuestionGroupForQuestionsPaginationDto
    {
        public Guid Id { get; init; }

        public Guid? BranchId { get; init; }
    }
}
