using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;
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

        [HttpGet("branch-dashboard")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetBranchDashboard(
    [FromQuery] GetBranchDashboardQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetBranchDashboardQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("department-dashboard")]
        [Permission("Reports.ViewDepartmentReports")]
        public async Task<IActionResult> GetDepartmentDashboard(
    [FromQuery] GetDepartmentDashboardQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetDepartmentDashboardQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("branch-responses/{surveyResponseId:guid}")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetBranchSurveyResponseDetails(
    [FromRoute] Guid surveyResponseId,
    CancellationToken cancellationToken)
        {
            var query = new GetBranchSurveyResponseDetailsQuery
            {
                SurveyResponseId = surveyResponseId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("branch-responses")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetBranchResponses(
    [FromQuery] GetBranchSurveyResponsesPaginationQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetBranchSurveyResponsesPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("system-dashboard")]
        [Permission("Reports.ViewSystemDashboard")]
        public async Task<IActionResult> GetSystemDashboard(
    [FromQuery] GetSystemDashboardQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetSystemDashboardQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("system-responses/{surveyResponseId:guid}")]
        [Permission("Reports.ViewSystemDashboard")]
        public async Task<IActionResult> GetSystemSurveyResponseDetails(
    [FromRoute] Guid surveyResponseId,
    CancellationToken cancellationToken)
        {
            var query = new GetSystemSurveyResponseDetailsQuery
            {
                SurveyResponseId = surveyResponseId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("system-responses")]
        [Permission("Reports.ViewSystemDashboard")]
        public async Task<IActionResult> GetSystemResponses(
    [FromQuery] GetSystemSurveyResponsesPaginationQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetSystemSurveyResponsesPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }
    }
}
