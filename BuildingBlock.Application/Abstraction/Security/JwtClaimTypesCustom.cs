namespace BuildingBlock.Application.Abstraction.Security
{
    public static class JwtClaimTypesCustom
    {
        public const string UserId = "userId";
        public const string AccountId = "accountId";

        public const string Email = "email";
        public const string PhoneNumber = "phoneNumber";

        public const string Permission = "permission";

        // Source of truth for high-level auth
        public const string UserType = "userType";
        public const string ActiveBranchId = "activeBranchId";

        public const string Role = "role"; // optional if you still issue role claim
    }
}
