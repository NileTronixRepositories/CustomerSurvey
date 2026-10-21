using System.Reflection;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Api.Controllers;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using MediatR;
using Xunit;

namespace CustomerSurvey.Tests.Reports;

public sealed class ReportingControllerContractTests
{
    [Fact]
    public async Task Anonymous_response_route_forwards_mode_and_drilldown_flags_and_preserves_path_identity()
    {
        var sender = DispatchProxy.Create<ISender, SenderProxy>();
        var capture = (SenderProxy)(object)sender;
        var controller = new AnonymousTemplatesController(sender);
        var templateId = Guid.NewGuid();
        await controller.GetResponses(templateId, new GetAnonymousTemplateResponsesPaginationQuery
        {
            AnonymousTemplateId = Guid.NewGuid(), ScoreCalculationMode = ScoreCalculationMode.AllAnsweredQuestions,
            ScoreIncludedOnly = true, ComplaintItemsOnly = true, PageNumber = 1, PageSize = 10
        }, CancellationToken.None);
        var request = Assert.IsType<GetAnonymousTemplateResponsesPaginationQuery>(capture.Request);
        Assert.Equal(templateId, request.AnonymousTemplateId);
        Assert.Equal(ScoreCalculationMode.AllAnsweredQuestions, request.ScoreCalculationMode);
        Assert.True(request.ScoreIncludedOnly);
        Assert.True(request.ComplaintItemsOnly);
    }

    public class SenderProxy : DispatchProxy
    {
        public object? Request { get; private set; }
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Request = args![0];
            return Task.FromResult(Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Ok(
                new Pagination<AnonymousTemplateResponsePaginationItemResponse>(1, 10, 0, Array.Empty<AnonymousTemplateResponsePaginationItemResponse>())));
        }
    }
}
