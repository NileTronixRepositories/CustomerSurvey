using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin
{
    public sealed record CreateBranchAdminCommand
        : ICommand<CreateBranchAdminResponse>
    {
        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string Password { get; init; } = string.Empty;
    }
}