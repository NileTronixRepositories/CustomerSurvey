namespace CustomerSurvey.Api.Contracts.AnonTemplates
{
    public sealed record SubmitAnonymousTemplateResponseRequest
    {
        public IReadOnlyCollection<SubmitAnonymousTemplateCustomInputValueRequest> CustomInputValues { get; init; }
            = Array.Empty<SubmitAnonymousTemplateCustomInputValueRequest>();

        public IReadOnlyCollection<SubmitAnonymousTemplateAnswerRequest> Answers { get; init; }
            = Array.Empty<SubmitAnonymousTemplateAnswerRequest>();
    }

    public sealed record SubmitAnonymousTemplateCustomInputValueRequest
    {
        public Guid CustomInputId { get; init; }

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }

    public sealed record SubmitAnonymousTemplateAnswerRequest
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }
    }
}