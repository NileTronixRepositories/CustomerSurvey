using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard
{
    public sealed class GetBranchDashboardQuery : IQuery<GetBranchDashboardResponse>
    {
        public DateOnly? From { get; init; }

        public DateOnly? To { get; init; }

        public Guid? TemplateId { get; init; }

        public BranchDashboardGroupBy GroupBy { get; init; } = BranchDashboardGroupBy.Day;

        public int TopQuestionsCount { get; init; } = 5;

        public int CriticalResponsesCount { get; init; } = 10;

        public decimal CriticalScoreThreshold { get; init; } = 40m;
    }

    public enum BranchDashboardGroupBy
    {
        Day = 1,
        Month = 2
    }
}