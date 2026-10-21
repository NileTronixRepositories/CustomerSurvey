namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    public sealed record AnonymousTemplateResponsePaginationItemResponse
    {
    public Guid? ComplaintQuestionId { get; init; }
    public string? ComplaintText { get; init; }

    public bool RequestedModeAvailable { get; init; } = true;
    public string ScoreDerivation { get; init; } = string.Empty;

        public Guid AnonymousSurveyResponseId { get; init; }

        public Guid AnonymousTemplateId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal ScorePercentage { get; init; }

        public bool IsScored { get; init; }

        public int AnswersCount { get; init; }

        public int CustomInputValuesCount { get; init; }
    }
}