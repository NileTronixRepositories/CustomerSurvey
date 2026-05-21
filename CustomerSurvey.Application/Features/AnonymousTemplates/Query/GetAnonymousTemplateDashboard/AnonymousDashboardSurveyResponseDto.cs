using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed record AnonymousDashboardSurveyResponseDto
{
    public Guid AnonymousSurveyResponseId { get; init; }

    public Guid AnonymousTemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public AnonymousTemplateScope Scope { get; init; }

    public TemplateStatus Status { get; init; }

    public bool IsActive { get; init; }

    public string PublicUrl { get; init; } = string.Empty;

    public string? QrCode { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }
}
