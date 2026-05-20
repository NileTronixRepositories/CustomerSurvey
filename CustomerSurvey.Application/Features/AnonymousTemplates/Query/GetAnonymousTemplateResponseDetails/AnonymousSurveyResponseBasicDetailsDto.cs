namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed record AnonymousSurveyResponseBasicDetailsDto
    {
        public Guid AnonymousSurveyResponseId { get; init; }

        public Guid AnonymousTemplateId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal ScorePercentage { get; init; }

        public int AnswersCount { get; init; }

        public int CustomInputValuesCount { get; init; }
    }
}