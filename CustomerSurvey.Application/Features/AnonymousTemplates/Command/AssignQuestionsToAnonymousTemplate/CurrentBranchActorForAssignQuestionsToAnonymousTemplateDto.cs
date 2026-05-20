namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    internal sealed record CurrentBranchActorForAssignQuestionsToAnonymousTemplateDto
    {
        public Guid BranchId { get; init; }
    }
}