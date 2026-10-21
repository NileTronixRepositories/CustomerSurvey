using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

internal static class DashboardScoreSelection
{
    public static SelectedResponseScore Select(string? payload, int originalMaxScore, decimal originalPercentage, ScoreCalculationMode mode)
    {
        var snapshot = SnapshotScoring.Read(payload);
        if (snapshot?.IsHistoricallyVerified != true)
            return new(originalMaxScore, originalPercentage, mode == ScoreCalculationMode.RootQuestions, "StoredRootBaseline");
        var tokens = SnapshotScoring.Tokens(snapshot, mode);
        return new(tokens.Count * 5, tokens.Count == 0 ? 0m : ReportScoreRounding.Round(tokens.Average(x => x.ScoreValue) * 20m), true, "SubmissionSnapshot");
    }

    public static QuestionScoreToken? Question(string? payload, Guid questionId, ScoreCalculationMode mode)
    {
        var snapshot = SnapshotScoring.Read(payload);
        return snapshot is null ? null : SnapshotScoring.Tokens(snapshot, mode).FirstOrDefault(x => x.QuestionId == questionId);
    }
}

internal sealed record SelectedResponseScore(int MaxScore, decimal ScorePercentage, bool RequestedModeAvailable, string ScoreDerivation);
