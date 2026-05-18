using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed record QuestionPaginationItemDto
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}