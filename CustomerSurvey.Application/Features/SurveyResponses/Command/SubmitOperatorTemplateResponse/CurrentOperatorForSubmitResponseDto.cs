using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record CurrentOperatorForSubmitResponseDto
    {
        public Guid OperatorId { get; init; }

        public Guid DepartmentId { get; init; }

        public Guid ApplicationUserId { get; init; }
    }
}