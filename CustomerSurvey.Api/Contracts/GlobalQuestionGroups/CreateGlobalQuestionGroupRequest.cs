namespace CustomerSurvey.Api.Contracts.GlobalQuestionGroups
{
    public sealed record CreateGlobalQuestionGroupRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}