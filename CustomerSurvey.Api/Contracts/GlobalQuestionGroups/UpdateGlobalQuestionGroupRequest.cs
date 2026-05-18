namespace CustomerSurvey.Api.Contracts.GlobalQuestionGroups
{
    public sealed record UpdateGlobalQuestionGroupRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }
}