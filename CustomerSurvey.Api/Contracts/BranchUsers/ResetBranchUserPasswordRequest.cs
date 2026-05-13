namespace CustomerSurvey.Api.Contracts.BranchUsers
{
    public sealed class ResetBranchUserPasswordRequest
    {
        public string NewPassword { get; init; } = string.Empty;
    }
}