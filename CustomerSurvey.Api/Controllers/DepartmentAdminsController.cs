using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Departments;
using CustomerSurvey.Application.Features.Departments.Command.CreateDepartmentAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/department-admins")]
    [Authorize]
    public sealed class DepartmentAdminsController : ControllerBase
    {
        private readonly ISender sender;

        public DepartmentAdminsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPost]
        [Permission("DepartmentAdmins.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDepartmentAdminRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateDepartmentAdminCommand
            {
                DepartmentId = request.DepartmentId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}