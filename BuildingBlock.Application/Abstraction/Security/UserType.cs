namespace BuildingBlock.Application.Abstraction.Security
{
    public enum UserType
    {
        Unknown = 0,

        SuperAdmin = 1,
        PlatformAdmin = 2,

        AccountAdmin = 3

        // لاحقًا: Staff, Instructor, Student...
    }
}