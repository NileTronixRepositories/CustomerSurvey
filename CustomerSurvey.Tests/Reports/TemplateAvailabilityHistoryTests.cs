using CustomerSurvey.Application.Features.Reports.Shared;
using CustomerSurvey.Domain.Entities;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class TemplateAvailabilityHistoryTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Template = Guid.NewGuid();
    private static TemplateAvailabilityEvent Event(int day, bool active, int from = 0, int? to = null, bool baseline = false)
        => TemplateAvailabilityEvent.Create(Template, 1, Guid.NewGuid(), baseline ? "BaselineInitialized" : "Changed",
            active, Start.AddDays(from), to.HasValue ? Start.AddDays(to.Value) : null, Start.AddDays(day), null, baseline);

    [Theory]
    [InlineData(0, 5, true)]
    [InlineData(5, 9, false)]
    [InlineData(9, 12, true)]
    [InlineData(12, 20, false)]
    public void Activation_intervals_intersect_schedules_and_natural_expiry(int from, int to, bool expected)
    {
        var events = new[] { Event(0, true, 2), Event(5, false), Event(9, true, 2, 12) };
        Assert.Equal(expected, TemplateAvailabilityIntervals.WasAvailable(events, Start.AddDays(from), Start.AddDays(to)));
    }

    [Fact]
    public void Editing_a_schedule_does_not_rewrite_past_availability()
    {
        var events = new[] { Event(0, true), Event(10, true, 20) };
        Assert.True(TemplateAvailabilityIntervals.WasAvailable(events, Start.AddDays(1), Start.AddDays(5)));
        Assert.False(TemplateAvailabilityIntervals.WasAvailable(events, Start.AddDays(11), Start.AddDays(15)));
    }

    [Fact]
    public void Legacy_baseline_never_invents_pre_capture_availability()
    {
        var events = new[] { Event(10, true, baseline: true) };
        Assert.False(TemplateAvailabilityIntervals.WasAvailable(events, Start, Start.AddDays(9)));
        Assert.True(TemplateAvailabilityIntervals.WasAvailable(events, Start.AddDays(10), Start.AddDays(11)));
    }

    [Fact]
    public void Unknown_history_and_inactive_creation_have_no_verified_availability()
    {
        Assert.False(TemplateAvailabilityIntervals.WasAvailable(Array.Empty<TemplateAvailabilityEvent>(), Start, Start.AddDays(10)));
        Assert.False(TemplateAvailabilityIntervals.WasAvailable(new[] { Event(0, false) }, Start, Start.AddDays(10)));
    }
}
