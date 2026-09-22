using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Abstraction.Reports;
using CustomerSurvey.infrastructure.Reports;
using System.Text.Json;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class BranchTemplatesPdfReportRequestTests
{
    [Fact]
    public void PreparePdfRequest_AlwaysIncludesResponseDetails()
    {
        var request = new BranchTemplatesPdfReportRequest
        {
            BranchId = Guid.NewGuid(),
            FromDate = new DateOnly(2026, 9, 1),
            ToDate = new DateOnly(2026, 9, 22),
            IncludeResponseDetails = false
        };

        var result = BranchTemplatesPdfReportService.PreparePdfRequest(request);

        Assert.True(result.IncludeResponseDetails);
        Assert.Equal(request.BranchId, result.BranchId);
        Assert.Equal(request.FromDate, result.FromDate);
        Assert.Equal(request.ToDate, result.ToDate);
    }

    [Fact]
    public void InternalPresentationData_RemainsExcludedFromJsonContract()
    {
        var model = new BranchTemplatesPdfReportModel
        {
            Responses = new[]
            {
                new BranchTemplatesReportResponse { ResponseId = Guid.NewGuid() }
            },
            Graphics = new BranchTemplatesReportGraphics { TotalResponses = 1 },
            CustomInputDefinitions = new[]
            {
                new BranchTemplatesReportCustomInputDefinition { CustomInputId = Guid.NewGuid() }
            }
        };

        var json = JsonSerializer.Serialize(model);

        Assert.DoesNotContain("\"Responses\":", json);
        Assert.DoesNotContain("\"Graphics\":", json);
        Assert.DoesNotContain("\"CustomInputDefinitions\":", json);
    }
}
