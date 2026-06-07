namespace CustomerSurvey.Application.Options
{
    public sealed class PasswordPolicyOptions
    {
        public const string SectionName = "PasswordPolicy";

        public int ExpiryDays { get; init; } = 30;
    }
}
