using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed record SatisfactionSurveyResponseFlatDto
    {
        public string? SnapshotPayload { get; init; }
        public CustomerSurvey.Application.Features.Reports.Shared.EvaluationCriteria Criteria { get; init; } = new();

        public Guid SurveyResponseId { get; init; }

        public Guid TemplateId { get; init; }

        public string TemplateNameEn { get; init; } = string.Empty;

        public string? TemplateNameAr { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal ScorePercentage { get; init; }
    }
}