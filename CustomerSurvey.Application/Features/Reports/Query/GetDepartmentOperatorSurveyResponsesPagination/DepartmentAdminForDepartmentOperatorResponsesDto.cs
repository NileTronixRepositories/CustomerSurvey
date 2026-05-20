namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed record DepartmentAdminForDepartmentOperatorResponsesDto
{
    public Guid DepartmentAdminId { get; init; }

    public Guid DepartmentId { get; init; }
}
