namespace CustomerSurvey.Api.Contracts.AnonymousTemplates
{
    public sealed record AssignQuestionsToAnonymousTemplateRequest
    {
        public IReadOnlyCollection<AssignQuestionToAnonymousTemplateRequestItem> Questions { get; init; }
            = Array.Empty<AssignQuestionToAnonymousTemplateRequestItem>();
    }

    public sealed record AssignQuestionToAnonymousTemplateRequestItem
    {
        public Guid QuestionId { get; init; }

        public int Order { get; init; }
    }
}