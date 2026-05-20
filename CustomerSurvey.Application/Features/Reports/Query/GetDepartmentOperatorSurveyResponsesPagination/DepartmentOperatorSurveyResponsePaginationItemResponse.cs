namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;

public sealed record DepartmentOperatorSurveyResponsePaginationItemResponse
{
    public Guid SurveyResponseId { get; init; }

    public Guid BranchId { get; init; }

    public string BranchNameEn { get; init; } = string.Empty;

    public string? BranchNameAr { get; init; }

    public string BranchCode { get; init; } = string.Empty;

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }

    public bool HasComplaint { get; init; }

    public bool HasVoice { get; init; }

    public IReadOnlyCollection<DepartmentOperatorSurveyResponseCustomInputPreviewResponse> CustomInputsPreview { get; set; }
        = Array.Empty<DepartmentOperatorSurveyResponseCustomInputPreviewResponse>();
}

public sealed record DepartmentOperatorSurveyResponseCustomInputPreviewResponse
{
    public string Name { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}
