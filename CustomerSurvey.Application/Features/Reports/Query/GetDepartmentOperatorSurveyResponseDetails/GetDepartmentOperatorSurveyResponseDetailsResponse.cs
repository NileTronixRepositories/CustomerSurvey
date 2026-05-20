using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

public sealed record GetDepartmentOperatorSurveyResponseDetailsResponse
{
    public Guid SurveyResponseId { get; init; }

    public DepartmentOperatorSurveyResponseBranchInfoResponse Branch { get; init; } = new();

    public DepartmentOperatorSurveyResponseTemplateInfoResponse Template { get; init; } = new();

    public DepartmentOperatorSurveyResponseOperatorInfoResponse Operator { get; init; } = new();

    public DateTime SubmittedOnUtc { get; init; }

    public DepartmentOperatorSurveyResponseScoreResponse Score { get; init; } = new();

    public int CustomInputsCount { get; init; }

    public int AnswersCount { get; init; }

    public IReadOnlyCollection<DepartmentOperatorSurveyResponseCustomInputResponse> CustomInputs { get; init; }
        = Array.Empty<DepartmentOperatorSurveyResponseCustomInputResponse>();

    public IReadOnlyCollection<DepartmentOperatorSurveyResponseAnswerResponse> Answers { get; init; }
        = Array.Empty<DepartmentOperatorSurveyResponseAnswerResponse>();
}

public sealed record DepartmentOperatorSurveyResponseBranchInfoResponse
{
    public Guid BranchId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }

    public string Code { get; init; } = string.Empty;
}

public sealed record DepartmentOperatorSurveyResponseTemplateInfoResponse
{
    public Guid TemplateId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

public sealed record DepartmentOperatorSurveyResponseOperatorInfoResponse
{
    public Guid OperatorId { get; init; }

    public string NameEn { get; init; } = string.Empty;

    public string? NameAr { get; init; }
}

public sealed record DepartmentOperatorSurveyResponseScoreResponse
{
    public int ActualScore { get; init; }

    public int MaxScore { get; init; }

    public decimal ScorePercentage { get; init; }

    public bool IsScored { get; init; }
}

public sealed record DepartmentOperatorSurveyResponseCustomInputResponse
{
    public Guid CustomInputId { get; init; }

    public string Name { get; init; } = string.Empty;

    public TemplateCustomInputType Type { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string? StringValue { get; init; }

    public int? IntegerValue { get; init; }

    public string DisplayValue { get; init; } = string.Empty;
}

public sealed record DepartmentOperatorSurveyResponseAnswerResponse
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
