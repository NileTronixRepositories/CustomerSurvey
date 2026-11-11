using Microsoft.AspNetCore.Http;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.Operators
{
    public sealed class SubmitOperatorTemplateResponseRequest
    {
        public List<SubmitOperatorTemplateCustomInputRequest> CustomInputs { get; init; } = new();

        public List<SubmitSurveyFlagRequest> Flags { get; init; } = new();

        public List<SubmitOperatorTemplateAnswerRequest> Answers { get; init; } = new();
    }

    public sealed class SubmitSurveyFlagRequest
    {
        public SurveyResponseFlagType Type { get; init; }
        public string? Brief { get; init; }
    }

    public sealed class SubmitOperatorTemplateCustomInputRequest
    {
        public Guid CustomInputId { get; init; }

        public string? Value { get; init; }
    }

    public sealed class SubmitOperatorTemplateAnswerRequest
    {
        public Guid QuestionId { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public IFormFile? VoiceFile { get; init; }

        public IFormFile? ImageFile { get; init; }
    }
}
