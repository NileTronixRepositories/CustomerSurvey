namespace CustomerSurvey.Api.Contracts.Departments
{
    public sealed class CreateDepartmentRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}