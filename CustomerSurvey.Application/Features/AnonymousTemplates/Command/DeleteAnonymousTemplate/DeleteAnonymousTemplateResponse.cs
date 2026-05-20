using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    public sealed record DeleteAnonymousTemplateResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public TemplateStatus Status { get; init; }

        public string StatusName { get; init; } = string.Empty;

        public bool IsActive { get; init; }
    }
}