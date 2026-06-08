using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record LatestSurveyAnswerForMyOperatorTemplateDto
    {
        public Guid SurveyResponseId { get; init; }

        public Guid QuestionId { get; init; }

        public QuestionType QuestionType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public string? SelectedOptionTextEn { get; init; }

        public string? SelectedOptionTextAr { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }

        public string? ImageFileName { get; init; }
    }
}
