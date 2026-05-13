// CustomerSurvey.Api/Controllers/BranchUsersController.cs

using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Branches;
using CustomerSurvey.Api.Contracts.BranchUsers;
using CustomerSurvey.Application.Features.BranchUsers.Command.AssignRolesToBranchUser;
using CustomerSurvey.Application.Features.BranchUsers.Command.CreateBranchUser;
using CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser;
using CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword;
using CustomerSurvey.Application.Features.BranchUsers.Command.RestoreBranchUser;
using CustomerSurvey.Application.Features.BranchUsers.Command.UpdateBranchUser;
using CustomerSurvey.Application.Features.BranchUsers.Query.GetBranchUsersPagination;
using CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/branch-users")]
    [Authorize]
    public sealed class BranchUsersController : ControllerBase
    {
        private readonly ISender sender;

        public BranchUsersController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("my-roles")]
        public async Task<IActionResult> GetMyRoles(
            CancellationToken cancellationToken)
        {
            var query = new GetMyBranchUserRolesQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet]
        [Permission("BranchUsers.ViewAll")]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] GetBranchUsersPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetBranchUsersPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("BranchUsers.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBranchUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateBranchUserCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                RoleIds = request.RoleIds
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{userId:guid}")]
        [Permission("BranchUsers.Update")]
        public async Task<IActionResult> Update(
            Guid userId,
            [FromBody] UpdateBranchUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateBranchUserCommand
            {
                ApplicationUserId = userId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{userId:guid}")]
        [Permission("BranchUsers.Delete")]
        public async Task<IActionResult> Delete(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteBranchUserCommand
            {
                ApplicationUserId = userId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{userId:guid}/reset-password")]
        [Permission("BranchUsers.ResetPassword")]
        public async Task<IActionResult> ResetPassword(
            Guid userId,
            [FromBody] ResetBranchUserPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ResetBranchUserPasswordCommand
            {
                ApplicationUserId = userId,
                NewPassword = request.NewPassword
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{userId:guid}/roles")]
        [Permission("BranchUsers.AssignRoles")]
        public async Task<IActionResult> AssignRoles(
            Guid userId,
            [FromBody] AssignRolesToBranchUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AssignRolesToBranchUserCommand
            {
                ApplicationUserId = userId,
                RoleIds = request.RoleIds
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{userId:guid}/restore")]
        [Permission("BranchUsers.Update")]
        public async Task<IActionResult> Restore(
    Guid userId,
    CancellationToken cancellationToken)
        {
            var command = new RestoreBranchUserCommand
            {
                ApplicationUserId = userId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}