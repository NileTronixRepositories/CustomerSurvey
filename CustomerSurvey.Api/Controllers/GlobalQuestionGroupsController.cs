using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.GlobalQuestionGroups;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.CreateGlobalQuestionGroup;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.DeleteGlobalQuestionGroup;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.RestoreGlobalQuestionGroup;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.UpdateGlobalQuestionGroup;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination;
using CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/global-question-groups")]
    [Authorize]
    public sealed class GlobalQuestionGroupsController : ControllerBase
    {
        private readonly ISender sender;

        public GlobalQuestionGroupsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("GlobalQuestionGroups.ViewAll")]
        public async Task<IActionResult> GetPaginated(
          [FromQuery] GetGlobalQuestionGroupsPaginationQuery query,
          CancellationToken cancellationToken)
        {
            query ??= new GetGlobalQuestionGroupsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("selection")]
        [Permission("GlobalQuestionGroups.ViewAll")]
        public async Task<IActionResult> GetSelection(
    CancellationToken cancellationToken)
        {
            var query = new GetGlobalQuestionGroupsSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("GlobalQuestionGroups.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateGlobalQuestionGroupRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateGlobalQuestionGroupCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{groupId:guid}")]
        [Permission("GlobalQuestionGroups.Update")]
        public async Task<IActionResult> Update(
           Guid groupId,
           [FromBody] UpdateGlobalQuestionGroupRequest request,
           CancellationToken cancellationToken)
        {
            var command = new UpdateGlobalQuestionGroupCommand
            {
                GroupId = groupId,
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{groupId:guid}/restore")]
        [Permission("GlobalQuestionGroups.Restore")]
        public async Task<IActionResult> Restore(
    Guid groupId,
    CancellationToken cancellationToken)
        {
            var command = new RestoreGlobalQuestionGroupCommand
            {
                GroupId = groupId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{groupId:guid}")]
        [Permission("GlobalQuestionGroups.Delete")]
        public async Task<IActionResult> Delete(
    Guid groupId,
    CancellationToken cancellationToken)
        {
            var command = new DeleteGlobalQuestionGroupCommand
            {
                GroupId = groupId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}