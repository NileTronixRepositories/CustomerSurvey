using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.BranchAreas;
using CustomerSurvey.Application.Features.BranchAreas.Command.AssignBranchesToBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Command.CreateBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Command.DeleteBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Command.RestoreBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Command.UpdateBranchArea;
using CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreaDetails;
using CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreasPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/branch-areas")]
    [Authorize]
    public sealed class BranchAreasController : ControllerBase
    {
        private readonly ISender sender;

        public BranchAreasController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("BranchAreas.ViewAll")]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] GetBranchAreasPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetBranchAreasPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpGet("{branchAreaId:guid}")]
        [Permission("BranchAreas.ViewDetails")]
        public async Task<IActionResult> GetById(
            Guid branchAreaId,
            CancellationToken cancellationToken)
        {
            var query = new GetBranchAreaDetailsQuery
            {
                BranchAreaId = branchAreaId
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("BranchAreas.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBranchAreaRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateBranchAreaCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                BranchIds = request.BranchIds
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPut("{branchAreaId:guid}")]
        [Permission("BranchAreas.Update")]
        public async Task<IActionResult> Update(
            Guid branchAreaId,
            [FromBody] UpdateBranchAreaRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateBranchAreaCommand
            {
                BranchAreaId = branchAreaId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpDelete("{branchAreaId:guid}")]
        [Permission("BranchAreas.Delete")]
        public async Task<IActionResult> Delete(
            Guid branchAreaId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteBranchAreaCommand
            {
                BranchAreaId = branchAreaId
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPut("{branchAreaId:guid}/branches")]
        [Permission("BranchAreas.AssignBranches")]
        public async Task<IActionResult> AssignBranches(
            Guid branchAreaId,
            [FromBody] AssignBranchesToBranchAreaRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AssignBranchesToBranchAreaCommand
            {
                BranchAreaId = branchAreaId,
                BranchIds = request.BranchIds
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }

        [HttpPut("{branchAreaId:guid}/restore")]
        [Permission("BranchAreas.Restore")]
        public async Task<IActionResult> Restore(
            Guid branchAreaId,
            CancellationToken cancellationToken)
        {
            var command = new RestoreBranchAreaCommand
            {
                BranchAreaId = branchAreaId
            };

            var result = await sender.Send(command, cancellationToken);
            return result.ToIActionResult();
        }
    }
}
