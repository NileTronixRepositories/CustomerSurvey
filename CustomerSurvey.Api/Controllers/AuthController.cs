using BuildingBlock.Api;
using CustomerSurvey.Api.Contracts.Auth;
using CustomerSurvey.Application.Features.Auth.Command.Login;
using MediatR;
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
    }
}