using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record TemplateQuestionForSubmitResponseDto
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public QuestionType Type { get; init; }

        public int Order { get; init; }
    }
}