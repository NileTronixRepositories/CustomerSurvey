namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed record QuestionOptionForSubmitDto
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Value { get; init; }

        public bool IsActive { get; init; }
    }
}