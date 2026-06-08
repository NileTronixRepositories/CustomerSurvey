namespace CustomerSurvey.Application.Features.Reports.Services.Scoring;

public static class ReportScoreRounding
{
    public static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
