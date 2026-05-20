using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    public sealed record GetAnonymousTemplateResponseDetailsResponse
    {
        public Guid AnonymousSurveyResponseId { get; init; }

        public Guid AnonymousTemplateId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal ScorePercentage { get; init; }

        public bool IsScored { get; init; }

        public int AnswersCount { get; init; }

        public int CustomInputValuesCount { get; init; }

        public IReadOnlyCollection<AnonymousTemplateResponseCustomInputValueDetailsResponse> CustomInputValues { get; init; }
            = Array.Empty<AnonymousTemplateResponseCustomInputValueDetailsResponse>();

        public IReadOnlyCollection<AnonymousTemplateResponseAnswerDetailsResponse> Answers { get; init; }
            = Array.Empty<AnonymousTemplateResponseAnswerDetailsResponse>();
    }

    public sealed record AnonymousTemplateResponseCustomInputValueDetailsResponse
    {
        public Guid CustomInputValueId { get; init; }

        public Guid AnonymousTemplateCustomInputId { get; init; }

        public string NameSnapshot { get; init; } = string.Empty;

        public TemplateCustomInputType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }

    public sealed record AnonymousTemplateResponseAnswerDetailsResponse
    {
        public Guid AnswerId { get; init; }

        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public string QuestionTextEn { get; init; } = string.Empty;

        public string? QuestionTextAr { get; init; }

        public QuestionType QuestionType { get; init; }

        public string QuestionTypeName { get; init; } = string.Empty;

        public int QuestionOrder { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public string? SelectedOptionTextEn { get; init; }

        public string? SelectedOptionTextAr { get; init; }

        public int? SelectedOptionValue { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }
    }
}