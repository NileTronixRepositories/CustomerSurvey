namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

internal sealed record DepartmentOperatorForResponsesDto
{
    public Guid OperatorId { get; init; }

    public Guid DepartmentId { get; init; }
}
