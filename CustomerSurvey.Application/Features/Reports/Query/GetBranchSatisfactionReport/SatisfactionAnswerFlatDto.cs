using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed record SatisfactionAnswerFlatDto
    {
        public Guid SurveyResponseId { get; init; }

        public Guid TemplateId { get; init; }

        public string TemplateNameEn { get; init; } = string.Empty;

        public string? TemplateNameAr { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public QuestionType QuestionType { get; init; }

        public int? StarRatingValue { get; init; }

        public int? SmileValue { get; init; }

        public string? TextAnswer { get; init; }

        public string? VoiceFileName { get; init; }
    }
}