using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public sealed class ReportsController : ControllerBase
    {
        private readonly ISender sender;

        public ReportsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("branch/satisfaction")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetBranchSatisfaction(
            [FromQuery] GetBranchSatisfactionReportQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetBranchSatisfactionReportQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }
    }
}