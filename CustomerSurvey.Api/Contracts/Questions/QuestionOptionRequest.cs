namespace CustomerSurvey.Api.Contracts.Questions
{
    public sealed class QuestionOptionRequest
    {
        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }
    }
}