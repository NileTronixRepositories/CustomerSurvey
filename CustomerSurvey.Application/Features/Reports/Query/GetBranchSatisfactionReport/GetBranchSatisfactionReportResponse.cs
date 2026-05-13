using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    public sealed record GetBranchSatisfactionReportResponse
    {
        public SatisfactionReportPeriodResponse Period { get; init; } = new();

        public SatisfactionOverallResponse Overall { get; init; } = new();

        public IReadOnlyCollection<SatisfactionDistributionItemResponse> Distribution { get; init; }
            = Array.Empty<SatisfactionDistributionItemResponse>();

        public IReadOnlyCollection<SatisfactionByTemplateItemResponse> ByTemplate { get; init; }
            = Array.Empty<SatisfactionByTemplateItemResponse>();

        public IReadOnlyCollection<SatisfactionTrendItemResponse> Trend { get; init; }
            = Array.Empty<SatisfactionTrendItemResponse>();

        public SatisfactionComplaintsResponse Complaints { get; init; } = new();
    }

    public sealed record SatisfactionReportPeriodResponse
    {
        public DateOnly From { get; init; }

        public DateOnly To { get; init; }

        public bool IsDefaultPeriod { get; init; }

        public string PeriodSource { get; init; } = string.Empty;
    }

    public sealed record SatisfactionOverallResponse
    {
        public decimal Score { get; init; }

        public int TotalResponses { get; init; }

        public int ScoredResponses { get; init; }

        public int UnscoredResponses { get; init; }

        public int TotalScoredAnswers { get; init; }

        public int SatisfiedResponses { get; init; }

        public int NeutralResponses { get; init; }

        public int UnsatisfiedResponses { get; init; }

        public int ComplaintsCount { get; init; }

        public int VoiceAnswersCount { get; init; }
    }

    public sealed record SatisfactionDistributionItemResponse
    {
        public int Value { get; init; }

        public string LabelEn { get; init; } = string.Empty;

        public string LabelAr { get; init; } = string.Empty;

        public int Count { get; init; }

        public decimal Percentage { get; init; }
    }

    public sealed record SatisfactionByTemplateItemResponse
    {
        public Guid TemplateId { get; init; }

        public string TemplateNameEn { get; init; } = string.Empty;

        public string? TemplateNameAr { get; init; }

        public decimal Score { get; init; }

        public int ResponsesCount { get; init; }

        public int ScoredAnswersCount { get; init; }
    }

    public sealed record SatisfactionTrendItemResponse
    {
        public DateOnly Date { get; init; }

        public decimal Score { get; init; }

        public int ResponsesCount { get; init; }
    }

    public sealed record SatisfactionComplaintsResponse
    {
        public int Count { get; init; }

        public decimal PercentageOfResponses { get; init; }
    }
}