namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    internal sealed record SelectedAnonymousTemplateQuestionForSelectionDto
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public bool IsRequired { get; init; } = true;

        public int Order { get; init; }
    }
}