using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed record AnonymousTemplateForSubmitResponseDto
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public bool IsActive { get; init; }
    }
}