using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.CreateOperator
{
    public sealed record CreateOperatorResponse
    {
        public Guid ApplicationUserId { get; init; }

        public Guid OperatorId { get; init; }

        public Guid DepartmentId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}