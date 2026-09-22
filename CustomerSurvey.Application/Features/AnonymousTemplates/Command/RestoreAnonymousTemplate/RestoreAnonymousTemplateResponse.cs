using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    public sealed record RestoreAnonymousTemplateResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsActive { get; init; }

        public bool IsArchived { get; init; }
    }
}
