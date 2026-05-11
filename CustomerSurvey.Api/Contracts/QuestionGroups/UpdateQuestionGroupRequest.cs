namespace CustomerSurvey.Api.Contracts.QuestionGroups
{
    public sealed class UpdateQuestionGroupRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}