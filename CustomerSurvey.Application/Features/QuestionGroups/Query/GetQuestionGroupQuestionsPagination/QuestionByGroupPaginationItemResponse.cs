using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    public sealed record QuestionByGroupPaginationItemResponse
    {
        public Guid Id { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}
