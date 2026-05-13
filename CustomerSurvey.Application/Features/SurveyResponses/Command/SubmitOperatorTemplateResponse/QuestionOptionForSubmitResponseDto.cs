using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record QuestionOptionForSubmitResponseDto
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }
    }
}