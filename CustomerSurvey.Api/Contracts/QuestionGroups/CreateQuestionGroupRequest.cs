namespace CustomerSurvey.Api.Contracts.QuestionGroups
{
    public sealed class CreateQuestionGroupRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}