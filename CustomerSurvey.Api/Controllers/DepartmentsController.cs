using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Departments;
using CustomerSurvey.Application.Features.Departments.Command.CreateDepartment;
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
    }
}