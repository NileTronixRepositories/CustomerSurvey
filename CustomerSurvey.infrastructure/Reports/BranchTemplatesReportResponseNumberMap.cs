using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;

namespace CustomerSurvey.infrastructure.Reports;

internal sealed class BranchTemplatesReportResponseNumberMap
{
    private readonly IReadOnlyDictionary<(ReportTemplateKindKey Kind, Guid ResponseId), int> _numbers;

    private BranchTemplatesReportResponseNumberMap(
        IReadOnlyDictionary<(ReportTemplateKindKey Kind, Guid ResponseId), int> numbers)
    {
        _numbers = numbers;
    }

    public static BranchTemplatesReportResponseNumberMap Create(
        IEnumerable<BranchTemplatesReportResponse> responses)
    {
        ArgumentNullException.ThrowIfNull(responses);

        var ordered = responses
            .OrderBy(x => x.SubmittedOnUtc)
            .ThenBy(x => x.ResponseId)
            .ThenBy(x => x.TemplateKind)
            .ToArray();

        var numbers = ordered
            .Select((response, index) => new
            {
                Key = (ToKey(response), response.ResponseId),
                Number = index + 1
            })
            .ToDictionary(x => x.Key, x => x.Number);

        return new BranchTemplatesReportResponseNumberMap(numbers);
    }

    public int GetNumber(BranchTemplatesReportResponse response)
        => _numbers[(ToKey(response), response.ResponseId)];

    private static ReportTemplateKindKey ToKey(BranchTemplatesReportResponse response)
        => (ReportTemplateKindKey)(int)response.TemplateKind;

    private enum ReportTemplateKindKey
    {
        Normal = 1,
        Anonymous = 2
    }
}
