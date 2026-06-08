using Microsoft.AspNetCore.Http;

namespace CustomerSurvey.Api.Contracts.AnonTemplates
{
    public sealed record SubmitAnonymousTemplateResponseRequest
    {
        public List<SubmitAnonymousTemplateCustomInputValueRequest> CustomInputValues { get; init; } = new();

        public List<SubmitAnonymousTemplateAnswerRequest> Answers { get; init; } = new();
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

        public IFormFile? ImageFile { get; init; }
    }
}
