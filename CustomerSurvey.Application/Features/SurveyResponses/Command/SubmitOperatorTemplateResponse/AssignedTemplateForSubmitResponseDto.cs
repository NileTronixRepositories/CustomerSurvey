using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record AssignedTemplateForSubmitResponseDto
    {
        public Guid TemplateId { get; init; }

        public bool TemplateIsActive { get; init; }
    }
}