using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Branches;
using CustomerSurvey.Application.Features.Branches.Command.CreateBranch;
using CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/branches")]
    [Authorize]
    public sealed class BranchesController : ControllerBase
    {
        private readonly ISender sender;

        public BranchesController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPost]
        [Permission("Branches.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBranchRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateBranchCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Code = request.Code,
                Address = request.Address
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost("branch-admins-create")]
        [Permission("BranchAdmins.Create")]
        public async Task<IActionResult> Create(
           [FromBody] CreateBranchAdminRequest request,
           CancellationToken cancellationToken)
        {
            var command = new CreateBranchAdminCommand
            {
                BranchId = request.BranchId,
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