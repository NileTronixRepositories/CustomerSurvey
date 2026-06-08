using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin
{
    public sealed record CreateSuperAdminCommand : ICommand<CreateSuperAdminResponse>
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string Password { get; init; } = string.Empty;
    }
}
