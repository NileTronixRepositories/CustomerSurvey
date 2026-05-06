namespace BuildingBlock.Application.Abstraction.Security
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }

        Guid? UserId { get; }
        Guid? AccountId { get; }

        string? Role { get; }

        // مصدر الحقيقة بدل IsPlatformAdmin
        UserType UserType { get; }
    }
}