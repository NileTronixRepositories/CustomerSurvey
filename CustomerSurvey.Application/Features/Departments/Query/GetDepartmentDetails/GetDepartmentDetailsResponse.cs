using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    public sealed record GetDepartmentDetailsResponse
    {
        public Guid Id { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public DepartmentDetailsSummaryResponse Summary { get; init; } = new();

        public IReadOnlyCollection<DepartmentDetailsDepartmentAdminResponse> DepartmentAdmins { get; init; }
            = Array.Empty<DepartmentDetailsDepartmentAdminResponse>();

        public IReadOnlyCollection<DepartmentDetailsOperatorResponse> Operators { get; init; }
            = Array.Empty<DepartmentDetailsOperatorResponse>();
    }

    public sealed record DepartmentDetailsSummaryResponse
    {
        public int DepartmentAdminsCount { get; init; }

        public int OperatorsCount { get; init; }
    }

    public sealed record DepartmentDetailsDepartmentAdminResponse
    {
        public Guid DepartmentAdminId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }

    public sealed record DepartmentDetailsOperatorResponse
    {
        public Guid OperatorId { get; init; }

        public Guid ApplicationUserId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }
    }
}