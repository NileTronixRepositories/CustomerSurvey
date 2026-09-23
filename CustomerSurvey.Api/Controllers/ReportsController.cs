using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchAnonymousResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplateExcelReport;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetDepartmentSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;
using CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponsesPagination;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboardTemplatesSelection;
using CustomerSurvey.Application.Features.Reports.Query.GetSurveyResponsesPagination;
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

        [HttpGet("department-responses")]
        [Permission("Reports.ViewDepartmentReports")]
        public async Task<IActionResult> GetDepartmentResponses(
            [FromQuery] GetDepartmentSurveyResponsesPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetDepartmentSurveyResponsesPaginationQuery();
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

        [HttpGet("survey-dashboard")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetSurveyDashboard(
    [FromQuery] GetSurveyDashboardQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetSurveyDashboardQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("survey-dashboard/templates-selection")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetSurveyDashboardTemplatesSelection(
    [FromQuery] GetSurveyDashboardTemplatesSelectionQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetSurveyDashboardTemplatesSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("survey-responses")]
        [Permission("Reports.ViewBranchReports")]
        public async Task<IActionResult> GetSurveyResponses(
            [FromQuery] GetSurveyResponsesPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetSurveyResponsesPaginationQuery();
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
            query = query with
            {
                Language = NormalizeTemplatesReportLanguage(Request.Headers.AcceptLanguage.ToString())
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

            Response.Headers.Append("X-Pdf-File-Name", Uri.EscapeDataString(result.Value.FileName));
            Response.Headers.Append("X-Pdf-Size", result.Value.Content.Length.ToString());

            return File(
                fileContents: result.Value.Content,
                contentType: "application/pdf",
                fileDownloadName: result.Value.FileName);
        }

        [HttpGet("templates/excel")]
        [Permission("Reports.ViewBranchReports")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        public async Task<IActionResult> GetBranchTemplateExcelReport(
            [FromQuery] GetBranchTemplateExcelReportQuery query,
            CancellationToken cancellationToken)
        {
            query = query with
            {
                Language = NormalizeTemplatesReportLanguage(Request.Headers.AcceptLanguage.ToString())
            };

            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
            {
                return result.ToIActionResult();
            }

            if (result.Value.Content is null || result.Value.Content.Length == 0)
            {
                return Problem(
                    title: "Excel generation failed",
                    detail: "Generated Excel content is empty.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            Response.Headers.Append("X-Excel-File-Name", Uri.EscapeDataString(result.Value.FileName));
            Response.Headers.Append("X-Excel-Size", result.Value.Content.Length.ToString());

            return File(
                fileContents: result.Value.Content,
                contentType: result.Value.ContentType,
                fileDownloadName: result.Value.FileName);
        }

        [HttpGet("templates")]
        [Permission("Reports.ViewBranchReports")]
        [Produces("application/json")]
        public async Task<IActionResult> GetBranchTemplatesReport(
     [FromQuery] GetBranchTemplatesReportQuery query,
     CancellationToken cancellationToken)
        {
            query = query with
            {
                Language = NormalizeTemplatesReportLanguage(Request.Headers.AcceptLanguage.ToString())
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        private static string NormalizeTemplatesReportLanguage(string language)
        {
            return language.StartsWith("ar", StringComparison.OrdinalIgnoreCase)
                ? "ar"
                : "en";
        }
    }
}
