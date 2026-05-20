using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

public sealed record GetBranchSurveyResponseDetailsResponse
{
    public Guid SurveyResponseId { get; init; }

    public Guid TemplateId { get; init; }

    public string TemplateNameEn { get; init; } = string.Empty;

    public string? TemplateNameAr { get; init; }

    public Guid OperatorId { get; init; }

    public string OperatorNameEn { get; init; } = string.Empty;

    public string? OperatorNameAr { get; init; }

    public DateTime SubmittedOnUtc { get; init; }

    public BranchSurveyResponseScoreResponse Score { get; init; } = new();

    public int CustomInputsCount { get; init; }

    public int AnswersCount { get; init; }

    public IReadOnlyCollection<BranchSurveyResponseCustomInputResponse> CustomInputs { get; init; }
        = Array.Empty<BranchSurveyResponseCustomInputResponse>();

    public IReadOnlyCollection<BranchSurveyResponseAnswerResponse> Answers { get; init; }
        = Array.Empty<BranchSurveyResponseAnswerResponse>();
}

public sealed record BranchSurveyResponseScoreResponse
{
    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }
}

public sealed record BranchSurveyResponseCustomInputResponse
{
    public Guid CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public string DisplayValue { get; init; } = string.Empty;
}

public sealed record BranchSurveyResponseAnswerResponse
{
    public Guid QuestionId { get; init; }

    public string QuestionTextEn { get; init; } = string.Empty;

    public string? QuestionTextAr { get; init; }

    public QuestionType QuestionType { get; init; }

    public string QuestionTypeName { get; init; } = string.Empty;

    public Guid? SelectedQuestionOptionId { get; init; }

    public string? SelectedOptionTextEn { get; init; }

    public string? SelectedOptionTextAr { get; init; }

    public int? SelectedOptionValue { get; init; }

    public int? StarRatingValue { get; init; }

    public int? SmileValue { get; init; }

    public string? TextAnswer { get; init; }

    public string? VoiceFileName { get; init; }

    public string? VoiceFileUrl { get; init; }

    public string DisplayValue { get; init; } = string.Empty;
}