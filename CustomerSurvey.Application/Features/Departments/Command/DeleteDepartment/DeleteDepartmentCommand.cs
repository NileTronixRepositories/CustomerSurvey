using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment
{
    public sealed record DeleteDepartmentCommand
          : ICommand<DeleteDepartmentResponse>
    {
        public Guid DepartmentId { get; init; }
    }
}