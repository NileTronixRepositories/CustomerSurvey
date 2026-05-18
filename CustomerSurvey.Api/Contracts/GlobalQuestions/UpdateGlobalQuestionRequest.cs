using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.GlobalQuestions
{
    public sealed record UpdateGlobalQuestionRequest
    {
        public Guid GroupId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public IReadOnlyCollection<UpdateGlobalQuestionOptionRequest> Options { get; init; }
            = Array.Empty<UpdateGlobalQuestionOptionRequest>();
    }

    public sealed record UpdateGlobalQuestionOptionRequest
    {
        public Guid? OptionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }
}