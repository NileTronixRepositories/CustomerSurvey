using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Abstraction.Security
{
    public sealed record CurrentBranchScope
    {
        public Guid ApplicationUserId { get; init; }

        public Guid BranchId { get; init; }

        public UserType ActorType { get; init; }

        public Guid ActorProfileId { get; init; }
    }
}
