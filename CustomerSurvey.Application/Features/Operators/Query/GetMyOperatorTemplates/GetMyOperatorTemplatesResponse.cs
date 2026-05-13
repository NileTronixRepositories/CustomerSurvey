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

        public int QuestionsCount { get; init; }

        public bool HasAnswered { get; init; }

        public MyOperatorTemplateLatestResponse? LatestResponse { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<MyOperatorTemplateQuestionResponse>();
    }

    public sealed record MyOperatorTemplateQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public IReadOnlyCollection<MyOperatorQuestionOptionResponse> Options { get; init; }
            = Array.Empty<MyOperatorQuestionOptionResponse>();
    }

    public sealed record MyOperatorTemplateLatestResponse
    {
        public Guid SurveyResponseId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int AnswersCount { get; init; }

        public IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> Answers { get; init; }
            = Array.Empty<MyOperatorTemplateLatestAnswerResponse>();
    }

    public sealed record MyOperatorTemplateLatestAnswerResponse
    {
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
    }
}