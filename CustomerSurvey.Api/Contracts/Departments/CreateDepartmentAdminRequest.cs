namespace CustomerSurvey.Api.Contracts.Departments
{
    public sealed class CreateDepartmentAdminRequest
    {
        public Guid DepartmentId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string Password { get; init; } = string.Empty;
    }
}