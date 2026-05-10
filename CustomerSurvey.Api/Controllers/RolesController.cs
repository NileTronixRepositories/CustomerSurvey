using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Application.Features.Roles.Query.GetRolesForSelection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public sealed class RolesController : ControllerBase
    {
        private readonly ISender sender;

        public RolesController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("selection")]
        [Permission("Roles.ViewSelection")]
        public async Task<IActionResult> GetSelection(
            CancellationToken cancellationToken)
        {
            var query = new GetRolesForSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }
    }
}