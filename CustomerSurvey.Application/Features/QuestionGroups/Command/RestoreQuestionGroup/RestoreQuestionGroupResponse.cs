using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    public sealed record RestoreQuestionGroupResponse
    {
        public Guid GroupId { get; init; }

        public Guid? BranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }
    }
}