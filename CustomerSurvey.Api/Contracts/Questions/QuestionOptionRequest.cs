namespace CustomerSurvey.Api.Contracts.Questions
{
    public sealed class QuestionOptionRequest
    {
        // Used only in Update Question.
        // For Create Question, frontend can send null / omit it.
        public Guid? OptionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        // New: required for SingleChoice options.
        public int Value { get; init; }
    }
}