namespace CustomerSurvey.Api.Contracts.Departments
{
    public sealed class CreateDepartmentRequest
    {
        public Guid? BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string Code { get; init; } = string.Empty;
    }
}