using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    internal sealed record OperatorPaginationItemDto
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public Guid DepartmentId { get; init; }

        public string DepartmentNameEn { get; init; } = string.Empty;

        public string? DepartmentNameAr { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }
    }
}