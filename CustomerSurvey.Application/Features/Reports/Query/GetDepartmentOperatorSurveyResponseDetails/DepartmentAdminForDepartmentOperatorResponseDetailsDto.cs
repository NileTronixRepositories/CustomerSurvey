namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed record DepartmentAdminForDepartmentOperatorResponseDetailsDto
{
    public Guid DepartmentAdminId { get; init; }

    public Guid DepartmentId { get; init; }
}
