using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

public sealed record GetSystemSurveyResponseDetailsResponse
{
    public Guid SurveyResponseId { get; init; }

    public SystemSurveyResponseBranchInfoResponse Branch { get; init; } = new();

    public SystemSurveyResponseDepartmentInfoResponse Department { get; init; } = new();

    public SystemSurveyResponseTemplateInfoResponse Template { get; init; } = new();

    public SystemSurveyResponseOperatorInfoResponse Operator { get; init; } = new();

    public DateTime SubmittedOnUtc { get; init; }

    public SystemSurveyResponseScoreResponse Score { get; init; } = new();

    public int CustomInputsCount { get; init; }

    public int AnswersCount { get; init; }

    public IReadOnlyCollection<SystemSurveyResponseCustomInputResponse> CustomInputs { get; init; }
        = Array.Empty<SystemSurveyResponseCustomInputResponse>();

    public IReadOnlyCollection<SystemSurveyResponseAnswerResponse> Answers { get; init; }
        = Array.Empty<SystemSurveyResponseAnswerResponse>();
}

public sealed record SystemSurveyResponseBranchInfoResponse
{
    public Guid BranchId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string Code { get; init; } = string.Empty;
}

public sealed record SystemSurveyResponseDepartmentInfoResponse
{
    public Guid DepartmentId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

public sealed record SystemSurveyResponseTemplateInfoResponse
{
    public Guid TemplateId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

public sealed record SystemSurveyResponseOperatorInfoResponse
{
    public Guid OperatorId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

public sealed record SystemSurveyResponseScoreResponse
{
    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }
}

public sealed record SystemSurveyResponseCustomInputResponse
{
    public Guid CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public string DisplayValue { get; init; } = string.Empty;
}

public sealed record SystemSurveyResponseAnswerResponse
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

    public string? ImageFileName { get; init; }

    public string? ImageFileUrl { get; init; }

    public string DisplayValue { get; init; } = string.Empty;
}
