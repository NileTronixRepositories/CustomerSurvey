using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed record SystemDashboardSurveyAnswerDto
{
    public Guid SurveyResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid BranchId { get; init; }

    public Guid DepartmentId { get; init; }

    public QuestionType QuestionType { get; init; }

    public string? TextAnswer { get; init; }
}