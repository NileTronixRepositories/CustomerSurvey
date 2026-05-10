using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Branches;
using CustomerSurvey.Application.Features.Branches.Command.CreateBranch;
using CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin;
using CustomerSurvey.Application.Features.Branches.Command.UpdateBranch;
using CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails;
using CustomerSurvey.Application.Features.Branches.Query.GetBranchesForSelection;
using CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination;
using CustomerSurvey.Application.Features.Branches.Query.GetMyBranchDetails;
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

        [HttpGet]
        [Permission("Branches.ViewAll")]
        public async Task<IActionResult> GetPaginated(
           [FromQuery] GetBranchesPaginationQuery query,
           CancellationToken cancellationToken)
        {
            query ??= new GetBranchesPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("selection")]
        [Permission("Branches.ViewAll")]
        public async Task<IActionResult> GetSelection(
    CancellationToken cancellationToken)
        {
            var query = new GetBranchesForSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("my-branch")]
        [Permission("Branches.ViewDetails")]
        public async Task<IActionResult> GetMyBranch(
    CancellationToken cancellationToken)
        {
            var query = new GetMyBranchDetailsQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{branchId:guid}")]
        [Permission("Branches.ViewDetails")]
        public async Task<IActionResult> GetById(
    Guid branchId,
    CancellationToken cancellationToken)
        {
            var query = new GetBranchDetailsQuery
            {
                BranchId = branchId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
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

        [HttpPut("{branchId:guid}")]
        [Permission("Branches.Update")]
        public async Task<IActionResult> Update(
    Guid branchId,
    [FromBody] UpdateBranchRequest request,
    CancellationToken cancellationToken)
        {
            var command = new UpdateBranchCommand
            {
                BranchId = branchId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Code = request.Code,
                Address = request.Address
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}