using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Departments;
using CustomerSurvey.Application.Features.Departments.Command.CreateDepartment;
using CustomerSurvey.Application.Features.Departments.Command.DeleteDepartment;
using CustomerSurvey.Application.Features.Departments.Command.UpdateDepartment;
using CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails;
using CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsForSelection;
using CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/departments")]
    [Authorize]
    public sealed class DepartmentsController : ControllerBase
    {
        private readonly ISender sender;

        public DepartmentsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("Departments.ViewAll")]
        public async Task<IActionResult> GetPaginated(
           [FromQuery] GetDepartmentsPaginationQuery query,
           CancellationToken cancellationToken)
        {
            query ??= new GetDepartmentsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("selection")]
        [Permission("Departments.ViewSelection")]
        public async Task<IActionResult> GetSelection(
            CancellationToken cancellationToken)
        {
            var query = new GetDepartmentsForSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{departmentId:guid}")]
        [Permission("Departments.ViewAll")]
        public async Task<IActionResult> GetById(
            Guid departmentId,
            CancellationToken cancellationToken)
        {
            var query = new GetDepartmentDetailsQuery
            {
                DepartmentId = departmentId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("Departments.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateDepartmentCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{departmentId:guid}")]
        [Permission("Departments.Update")]
        public async Task<IActionResult> Update(
          Guid departmentId,
          [FromBody] UpdateDepartmentRequest request,
          CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentCommand
            {
                DepartmentId = departmentId,
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{departmentId:guid}")]
        [Permission("Departments.Delete")]
        public async Task<IActionResult> Delete(
            Guid departmentId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteDepartmentCommand
            {
                DepartmentId = departmentId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}