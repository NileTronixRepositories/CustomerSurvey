using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;
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

        [HttpGet("department-operators/{operatorId:guid}/responses")]
        [Permission("Reports.ViewDepartmentReports")]
        public async Task<IActionResult> GetDepartmentOperatorResponses(
    [FromRoute] Guid operatorId,
    [FromQuery] GetDepartmentOperatorSurveyResponsesPaginationQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetDepartmentOperatorSurveyResponsesPaginationQuery();
            query.OperatorId = operatorId;
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("department-operators/{operatorId:guid}/responses/{surveyResponseId:guid}")]
        [Permission("Reports.ViewDepartmentReports")]
        public async Task<IActionResult> GetDepartmentOperatorResponseDetails(
    [FromRoute] Guid operatorId,
    [FromRoute] Guid surveyResponseId,
    CancellationToken cancellationToken)
        {
            var query = new GetDepartmentOperatorSurveyResponseDetailsQuery
            {
                OperatorId = operatorId,
                SurveyResponseId = surveyResponseId
            };

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

        [HttpGet("anonymous-responses")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetBranchAnonymousResponses(
    [FromQuery] GetBranchAnonymousResponsesPaginationQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetBranchAnonymousResponsesPaginationQuery();
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

        [HttpGet("templates/pdf")]
        [Permission("Reports.ViewBranchReports")]
        [Produces("application/pdf")]
        public async Task<IActionResult> GetBranchTemplatesPdfReport(
     [FromQuery] GetBranchTemplatesPdfReportQuery query,
     CancellationToken cancellationToken)
        {
            var language = Request.Headers.AcceptLanguage.ToString();

            var normalizedLanguage =
                language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
                    ? "ar"
                    : "en";

            query = query with
            {
                Language = normalizedLanguage
            };

            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToIActionResult();
            }

            if (result.Value.Content is null || result.Value.Content.Length == 0)
            {
                return Problem(
                    title: "PDF generation failed",
                    detail: "Generated PDF content is empty.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            Response.Headers.Append("X-Pdf-File-Name", result.Value.FileName);
            Response.Headers.Append("X-Pdf-Size", result.Value.Content.Length.ToString());

            return File(
                fileContents: result.Value.Content,
                contentType: "application/pdf",
                fileDownloadName: result.Value.FileName);
        }
    }
}
