using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Auth;
using CustomerSurvey.Application.Features.Auth.Command.ChangePassword;
using CustomerSurvey.Application.Features.Auth.Command.Login;
using CustomerSurvey.Application.Features.Auth.Command.ResetUserPassword;
using CustomerSurvey.Application.Features.Auth.Command.SelectBranch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly ISender sender;

        public AuthController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand
            {
                UserNameOrEmail = request.UserNameOrEmail,
                Password = request.Password
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost("select-branch")]
        [Authorize]
        public async Task<IActionResult> SelectBranch(
            [FromBody] SelectBranchRequest request,
            CancellationToken cancellationToken)
        {
            var command = new SelectBranchCommand
            {
                BranchId = request.BranchId
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPut("users/{applicationUserId:guid}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            Guid applicationUserId,
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ChangePasswordCommand
            {
                ApplicationUserId = applicationUserId,
                NewPassword = request.NewPassword,
                ConfirmNewPassword = request.ConfirmNewPassword
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPut("users/{applicationUserId:guid}/reset-password")]
        [Authorize]
        [Permission("Users.ResetPassword")]
        public async Task<IActionResult> ResetUserPassword(
            Guid applicationUserId,
            [FromBody] ResetUserPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ResetUserPasswordCommand
            {
                ApplicationUserId = applicationUserId,
                NewPassword = request.NewPassword,
                ConfirmNewPassword = request.ConfirmNewPassword
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}
