using System.Globalization;

namespace CustomerSurvey.Application.Features.Reports.Shared;

public sealed record DashboardDetailsNavigationResponse
{
    public string RouteType { get; init; } = string.Empty;

    public string Method { get; init; } = "GET";

    public string Path { get; init; } = string.Empty;
}

public sealed record DashboardChartsResponse
{
    public IReadOnlyCollection<SatisfactionDistributionItemResponse> SatisfactionDistribution { get; init; }
        = Array.Empty<SatisfactionDistributionItemResponse>();
}

public sealed record SatisfactionDistributionItemResponse
{
    public SatisfactionCategory Category { get; init; }

    public int ResponsesCount { get; init; }

    public decimal Percentage { get; init; }

    public DashboardDetailsNavigationResponse DetailsNavigation { get; init; } = new();
}

public sealed record DashboardSummaryActionsResponse
{
    public DashboardDetailsNavigationResponse AllResponses { get; init; } = new();

    public DashboardDetailsNavigationResponse Complaints { get; init; } = new();

    public DashboardDetailsNavigationResponse VoiceAnswers { get; init; } = new();
}

internal static class SatisfactionDistributionBuilder
{
    public static IReadOnlyCollection<SatisfactionDistributionItemResponse> Build<T>(
        IReadOnlyCollection<T> scoredResponses,
        Func<T, decimal> scoreSelector,
        Func<SatisfactionCategory, DashboardDetailsNavigationResponse> navigationFactory)
    {
        var denominator = scoredResponses.Count;

        return Enum.GetValues<SatisfactionCategory>()
            .Select(category =>
            {
                var count = scoredResponses.Count(response =>
                    SatisfactionCategoryRule.Matches(
                        maxScore: 1,
                        scoreSelector(response),
                        category));

                return new SatisfactionDistributionItemResponse
                {
                    Category = category,
                    ResponsesCount = count,
                    Percentage = denominator == 0
                        ? 0m
                        : Round((decimal)count / denominator * 100m),
                    DetailsNavigation = navigationFactory(category)
                };
            })
            .ToArray();
    }

    private static decimal Round(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

internal static class DashboardDrillDownPathBuilder
{
    public static DashboardDetailsNavigationResponse Navigation(
        string routeType,
        string path,
        params (string Name, object? Value)[] query)
        => new()
        {
            RouteType = routeType,
            Method = "GET",
            Path = Build(path, query)
        };

    public static string Build(
        string path,
        params (string Name, object? Value)[] query)
    {
        var values = query
            .Where(item => HasValue(item.Value))
            .Select(item =>
                $"{Uri.EscapeDataString(item.Name)}={Uri.EscapeDataString(Format(item.Value!))}")
            .ToArray();

        return values.Length == 0
            ? path
            : $"{path}?{string.Join("&", values)}";
    }

    public static (DateOnly From, DateOnly To) ResolveTrendRange(
        string period,
        bool isMonthly,
        DateOnly dashboardFrom,
        DateOnly dashboardTo)
    {
        if (!isMonthly)
        {
            var day = DateOnly.ParseExact(period, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return (day, day);
        }

        var month = DateOnly.ParseExact($"{period}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var monthEnd = month.AddMonths(1).AddDays(-1);

        return (
            month < dashboardFrom ? dashboardFrom : month,
            monthEnd > dashboardTo ? dashboardTo : monthEnd);
    }

    private static bool HasValue(object? value)
        => value is not null &&
           (value is not string text || !string.IsNullOrWhiteSpace(text));

    private static string Format(object value)
        => value switch
        {
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool boolean => boolean ? "true" : "false",
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
}
