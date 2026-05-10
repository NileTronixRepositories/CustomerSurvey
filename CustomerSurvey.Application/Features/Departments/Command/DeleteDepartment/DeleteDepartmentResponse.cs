using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment
{
    public sealed record DeleteDepartmentResponse
    {
        public Guid DepartmentId { get; init; }

        public bool IsActive { get; init; }
    }
}