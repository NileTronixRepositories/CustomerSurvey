namespace BuildingBlock.Application.Abstraction.Security
{
    public sealed class TokenInfoDto
    {
        public string RawToken { get; set; } = string.Empty;

        public Guid? UserId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? ActiveBranchId { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }
        public UserType UserType { get; set; } = UserType.Unknown;
        public int? UserTypeValue { get; set; }

        public string[] Permissions { get; set; } = Array.Empty<string>();

        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public DateTime? IssuedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string? Subject { get; set; }
        public string? JwtId { get; set; }

        public bool HasAccount => AccountId.HasValue && AccountId.Value != Guid.Empty;
    }
}
