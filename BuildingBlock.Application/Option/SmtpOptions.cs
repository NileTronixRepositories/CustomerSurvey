namespace BuildingBlock.Application.Option
{
    public sealed class SmtpOptions
    {
        public string Host { get; init; } = default!;
        public int Port { get; init; } = 587;
        public bool UseSsl { get; init; } = false;

        public string UserName { get; init; } = default!;
        public string Password { get; init; } = default!;

        public string FromEmail { get; init; } = default!;
        public string FromName { get; init; } = "Finance Management";
    }
}