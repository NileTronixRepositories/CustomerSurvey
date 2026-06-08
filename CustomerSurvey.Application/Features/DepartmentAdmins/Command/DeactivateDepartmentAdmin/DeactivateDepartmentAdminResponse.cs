namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin
{
    public sealed record DeactivateDepartmentAdminResponse
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public bool IsActive { get; init; }
    }
}
