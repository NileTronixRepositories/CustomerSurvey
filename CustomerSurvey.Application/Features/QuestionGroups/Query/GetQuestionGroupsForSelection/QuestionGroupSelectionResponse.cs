using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    public sealed record QuestionGroupSelectionResponse
    {
        public Guid Id { get; init; }

        public Guid? BranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsSelectable { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}