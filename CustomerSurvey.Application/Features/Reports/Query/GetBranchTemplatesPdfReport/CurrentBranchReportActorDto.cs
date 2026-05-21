namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

internal sealed record CurrentBranchReportActorDto
{
    public Guid ApplicationUserId { get; init; }

    public Guid BranchId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string DisplayName(bool isArabic)
        => isArabic && !string.IsNullOrWhiteSpace(NameAr) ? NameAr! : NameEn;
}