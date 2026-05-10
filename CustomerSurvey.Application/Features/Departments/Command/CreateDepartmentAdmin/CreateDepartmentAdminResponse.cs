using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin
{
    public sealed record CreateDepartmentAdminResponse
    {
        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentAdminId { get; init; }

        public Guid DepartmentId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}