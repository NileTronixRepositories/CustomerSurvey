namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

internal sealed record DepartmentOperatorForResponseDetailsDto
{
    public Guid OperatorId { get; init; }

    public Guid DepartmentId { get; init; }
}
