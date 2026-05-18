namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination
{
    public sealed record DepartmentPaginationItemResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public DepartmentPaginationCreatedByResponse? CreatedBy { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }

    public sealed record DepartmentPaginationCreatedByResponse
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}