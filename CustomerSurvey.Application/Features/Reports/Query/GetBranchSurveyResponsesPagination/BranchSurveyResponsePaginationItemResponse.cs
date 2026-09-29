namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;

public sealed record BranchSurveyResponsePaginationItemResponse
{
    public Guid SurveyResponseId { get; init; }

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

    public IReadOnlyCollection<BranchSurveyResponseCustomInputPreviewResponse> CustomInputsPreview { get; set; }
        = Array.Empty<BranchSurveyResponseCustomInputPreviewResponse>();
}

public sealed record BranchSurveyResponseCustomInputPreviewResponse
{
    public Guid CustomInputId { get; init; }

    public string? LabelEnSnapshot { get; init; }

    public string? LabelArSnapshot { get; init; }

    public string Value { get; init; } = string.Empty;
}
