using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin;
using CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/branch-admins")]
    [Authorize]
    public sealed class BranchAdminsController : ControllerBase
    {
        private readonly ISender sender;

        public BranchAdminsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPut("{branchAdminId:guid}/deactivate")]
        [Permission("BranchAdmins.Deactivate")]
        public async Task<IActionResult> Deactivate(
            Guid branchAdminId,
            CancellationToken cancellationToken)
        {
            var command = new DeactivateBranchAdminCommand
            {
                BranchAdminId = branchAdminId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{branchAdminId:guid}/restore")]
        [Permission("BranchAdmins.Restore")]
        public async Task<IActionResult> Restore(
            Guid branchAdminId,
            CancellationToken cancellationToken)
        {
            var command = new RestoreBranchAdminCommand
            {
                BranchAdminId = branchAdminId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}
