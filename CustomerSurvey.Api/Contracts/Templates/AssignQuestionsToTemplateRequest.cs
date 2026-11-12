namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class AssignQuestionsToTemplateRequest
    {
        public IReadOnlyCollection<Guid> QuestionIds { get; init; }
            = Array.Empty<Guid>();

        public IReadOnlyCollection<TemplateQuestionRequirementRequest?>? QuestionRequirements { get; init; }
    }

    public sealed record TemplateQuestionRequirementRequest
    {
        public Guid QuestionId { get; init; }
        public bool? IsRequired { get; init; }
    }
}