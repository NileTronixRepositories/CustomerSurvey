using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.infrastructure.Reports;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesReportResponseNumberMapTests
{
    [Fact]
    public void Create_AssignsOneBasedDeterministicNumbersAcrossNormalAndAnonymousResponses()
    {
        var submitted = new DateTime(2026, 9, 20, 10, 0, 0, DateTimeKind.Utc);
        var first = Response("00000000-0000-0000-0000-000000000001", submitted, ReportTemplateKind.Normal);
        var second = Response("00000000-0000-0000-0000-000000000002", submitted, ReportTemplateKind.Anonymous);
        var third = Response("00000000-0000-0000-0000-000000000003", submitted.AddMinutes(1), ReportTemplateKind.Normal);

        var map = BranchTemplatesReportResponseNumberMap.Create(new[] { third, second, first });

        Assert.Equal(1, map.GetNumber(first));
        Assert.Equal(2, map.GetNumber(second));
        Assert.Equal(3, map.GetNumber(third));
        Assert.Equal(1, map.GetNumber(first));
    }

    private static BranchTemplatesReportResponse Response(
        string responseId,
        DateTime submitted,
        ReportTemplateKind kind)
        => new()
        {
            ResponseId = Guid.Parse(responseId),
            TemplateId = Guid.NewGuid(),
            TemplateKind = kind,
            SubmittedOnUtc = submitted
        };
}
