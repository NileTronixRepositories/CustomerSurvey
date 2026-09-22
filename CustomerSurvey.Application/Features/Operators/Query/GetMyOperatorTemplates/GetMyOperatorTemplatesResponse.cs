using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    public sealed record GetMyOperatorTemplatesResponse
    {
        public Guid OperatorId { get; init; }

        public Guid DepartmentId { get; init; }

        public int TemplatesCount { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateItemResponse> Templates { get; init; }
            = Array.Empty<MyOperatorTemplateItemResponse>();
    }

    public sealed record MyOperatorTemplateItemResponse
    {
        public Guid TemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;

        public string? LogoPath { get; init; }

        public int QuestionsCount { get; init; }

        public int CustomInputsCount { get; init; }

        public bool HasAnswered { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public MyOperatorTemplateLatestResponse? LatestResponse { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateCustomInputResponse> CustomInputs { get; init; }
            = Array.Empty<MyOperatorTemplateCustomInputResponse>();

        public IReadOnlyCollection<MyOperatorTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<MyOperatorTemplateQuestionResponse>();

        public IReadOnlyCollection<TemplateQuestionConditionResponse> QuestionConditions { get; init; }
            = Array.Empty<TemplateQuestionConditionResponse>();
    }

    public sealed record MyOperatorTemplateCustomInputResponse
    {
        public Guid CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? LabelEn { get; init; }

        public string? LabelAr { get; init; }

        public TemplateCustomInputType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsRequired { get; init; }

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinValue { get; init; }

        public int? MaxValue { get; init; }

        public string? StartWith { get; init; }

        public int Order { get; init; }
    }

    public sealed record MyOperatorTemplateQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public IReadOnlyCollection<MyOperatorQuestionOptionResponse> Options { get; init; }
            = Array.Empty<MyOperatorQuestionOptionResponse>();
    }

    public sealed record MyOperatorQuestionOptionResponse
    {
        public Guid OptionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }

    public sealed record MyOperatorTemplateLatestResponse
    {
        public Guid SurveyResponseId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int AnswersCount { get; init; }

        public int CustomInputsCount { get; init; }

        public MyOperatorTemplateLatestScoreResponse Score { get; init; } = new();

        public IReadOnlyCollection<MyOperatorTemplateLatestCustomInputValueResponse> CustomInputs { get; init; }
            = Array.Empty<MyOperatorTemplateLatestCustomInputValueResponse>();

        public IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> Answers { get; init; }
            = Array.Empty<MyOperatorTemplateLatestAnswerResponse>();
    }
    public sealed record MyOperatorTemplateLatestCustomInputValueResponse
    {
        public Guid CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public TemplateCustomInputType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }

    public sealed record MyOperatorTemplateLatestScoreResponse
    {
        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal Percentage { get; init; }
    }

    public sealed record MyOperatorTemplateLatestAnswerResponse
    {
        public Guid? TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public string QuestionType { get; init; } = string.Empty;

        public Guid? SelectedQuestionOptionId { get; init; }

        public string? SelectedOptionTextEn { get; init; }

        public string? SelectedOptionTextAr { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }

        public string? VoiceFileUrl { get; init; }

        public string? ImageFileName { get; init; }

        public string? ImageFileUrl { get; init; }
    }
}
