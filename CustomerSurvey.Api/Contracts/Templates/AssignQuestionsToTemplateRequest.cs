namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class AssignQuestionsToTemplateRequest
    {
        public IReadOnlyCollection<Guid> QuestionIds { get; init; }
            = Array.Empty<Guid>();
    }
}