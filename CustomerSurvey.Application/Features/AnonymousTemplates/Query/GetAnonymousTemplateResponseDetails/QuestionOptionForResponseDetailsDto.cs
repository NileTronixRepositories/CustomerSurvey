namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails
{
    internal sealed record QuestionOptionForResponseDetailsDto
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Value { get; init; }
    }
}