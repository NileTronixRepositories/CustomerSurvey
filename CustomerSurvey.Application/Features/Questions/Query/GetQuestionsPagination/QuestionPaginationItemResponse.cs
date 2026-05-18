using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    public sealed record QuestionPaginationItemResponse
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public IReadOnlyCollection<QuestionOptionResponse> Options { get; init; }
            = Array.Empty<QuestionOptionResponse>();
    }
}