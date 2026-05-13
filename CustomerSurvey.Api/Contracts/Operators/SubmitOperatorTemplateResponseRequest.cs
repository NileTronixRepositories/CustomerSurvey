namespace CustomerSurvey.Api.Contracts.Operators
{
    public sealed class SubmitOperatorTemplateResponseRequest
    {
        public List<SubmitOperatorTemplateAnswerRequest> Answers { get; init; } = new();
    }

    public sealed class SubmitOperatorTemplateAnswerRequest
    {
        public Guid QuestionId { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public IFormFile? VoiceFile { get; init; }
    }
}