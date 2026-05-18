using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    public sealed record DeleteQuestionResponse
    {
        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public bool IsActive { get; init; }
    }
}