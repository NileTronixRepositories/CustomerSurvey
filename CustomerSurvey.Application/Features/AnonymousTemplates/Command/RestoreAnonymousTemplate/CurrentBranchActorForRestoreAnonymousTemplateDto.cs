namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    internal sealed record CurrentBranchActorForRestoreAnonymousTemplateDto
    {
        public Guid BranchId { get; init; }
    }
}