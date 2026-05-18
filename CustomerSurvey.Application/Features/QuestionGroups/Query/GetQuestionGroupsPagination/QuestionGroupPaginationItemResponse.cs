namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    public sealed record QuestionGroupPaginationItemResponse
    {
        public Guid GroupId { get; init; }

        public Guid? BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public int QuestionsCount { get; init; }

        public QuestionGroupPaginationCreatedByResponse? CreatedBy { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    public sealed record QuestionGroupPaginationCreatedByResponse
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}