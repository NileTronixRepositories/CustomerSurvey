using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.GlobalQuestions
{
    public sealed record CreateGlobalQuestionRequest
    {
        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public IReadOnlyCollection<CreateGlobalQuestionOptionRequest> Options { get; init; }
            = Array.Empty<CreateGlobalQuestionOptionRequest>();
    }

    public sealed record CreateGlobalQuestionOptionRequest
    {
        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }
}