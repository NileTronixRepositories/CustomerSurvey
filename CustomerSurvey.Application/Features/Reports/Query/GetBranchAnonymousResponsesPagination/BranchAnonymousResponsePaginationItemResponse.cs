namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;

public sealed record BranchAnonymousResponsePaginationItemResponse
{
    public Guid AnonymousSurveyResponseId { get; init; }

    public Guid AnonymousTemplateId { get; init; }

    public string AnonymousTemplateNameEn { get; init; } = string.Empty;

    public string? AnonymousTemplateNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }

    public bool HasComplaint { get; init; }

    public bool HasVoice { get; init; }

    public IReadOnlyCollection<BranchAnonymousResponseCustomInputPreviewResponse> CustomInputsPreview { get; init; }
        = Array.Empty<BranchAnonymousResponseCustomInputPreviewResponse>();
}

public sealed record BranchAnonymousResponseCustomInputPreviewResponse
{
    public Guid CustomInputId { get; init; }

    public string? LabelEnSnapshot { get; init; }

    public string? LabelArSnapshot { get; init; }

    public string Value { get; init; } = string.Empty;
}
