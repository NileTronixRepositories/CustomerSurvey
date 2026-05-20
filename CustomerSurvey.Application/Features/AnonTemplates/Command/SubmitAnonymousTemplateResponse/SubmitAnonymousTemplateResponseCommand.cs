using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    public sealed record SubmitAnonymousTemplateResponseCommand
        : ICommand<SubmitAnonymousTemplateResponseResult>
    {
        public Guid AnonymousTemplateId { get; init; }

        public IReadOnlyCollection<SubmitAnonymousTemplateCustomInputValueCommandItem> CustomInputValues { get; init; }
            = Array.Empty<SubmitAnonymousTemplateCustomInputValueCommandItem>();

        public IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> Answers { get; init; }
            = Array.Empty<SubmitAnonymousTemplateAnswerCommandItem>();
    }

    public sealed record SubmitAnonymousTemplateCustomInputValueCommandItem
    {
        public Guid CustomInputId { get; init; }

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }

    public sealed record SubmitAnonymousTemplateAnswerCommandItem
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }
    }
}