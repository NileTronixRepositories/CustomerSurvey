namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    public sealed record SubmitAnonymousTemplateResponseResult
    {
        public Guid AnonymousSurveyResponseId { get; init; }

        public Guid AnonymousTemplateId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public bool IsScored => MaxScore > 0;

        public decimal ScorePercentage { get; init; }

        public int VisibleQuestionsCount { get; init; }

        public int AnswersCount { get; init; }

        public int CustomInputValuesCount { get; init; }

        public IReadOnlyCollection<SubmittedAnonymousCustomInputValueResponse> CustomInputValues { get; init; } = [];
    }

    public sealed record SubmittedAnonymousCustomInputValueResponse
    {
        public Guid CustomInputId { get; init; }
        public string? StringValue { get; init; }
        public int? IntegerValue { get; init; }

        public DateOnly? DateValue { get; init; }
    }
}