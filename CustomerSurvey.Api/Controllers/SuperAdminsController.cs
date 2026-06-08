using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.SuperAdmins;
using CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/super-admins")]
    [Authorize]
    public sealed class SuperAdminsController : ControllerBase
    {
        private readonly ISender sender;

        public SuperAdminsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPost]
        [Permission("SuperAdmins.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateSuperAdminRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateSuperAdminCommand
            {
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
