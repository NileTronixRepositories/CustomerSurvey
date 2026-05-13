using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.Questions
{
    public sealed class UpdateQuestionRequest
    {
        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public IReadOnlyCollection<QuestionOptionRequest>? Options { get; init; }
    = Array.Empty<QuestionOptionRequest>();
    }
}