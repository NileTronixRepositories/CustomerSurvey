using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.QuestionGroups;
using CustomerSurvey.Application.Features.QuestionGroups.Command.CreateQuestionGroup;
using CustomerSurvey.Application.Features.QuestionGroups.Command.DeleteQuestionGroup;
using CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup;
using CustomerSurvey.Application.Features.QuestionGroups.Command.UpdateQuestionGroup;
using CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection;
using CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/question-groups")]
    [Authorize]
    public sealed class QuestionGroupsController : ControllerBase
    {
        private readonly ISender sender;

        public QuestionGroupsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("QuestionGroups.ViewAll")]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] GetQuestionGroupsPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetQuestionGroupsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("selection")]
        [Permission("QuestionGroups.ViewAll")]
        public async Task<IActionResult> GetSelection(
    CancellationToken cancellationToken)
        {
            var query = new GetQuestionGroupsForSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("QuestionGroups.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateQuestionGroupRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateQuestionGroupCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{groupId:guid}")]
        [Permission("QuestionGroups.Update")]
        public async Task<IActionResult> Update(
            Guid groupId,
            [FromBody] UpdateQuestionGroupRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateQuestionGroupCommand
            {
                GroupId = groupId,
                NameEn = request.NameEn,
                NameAr = request.NameAr
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{groupId:guid}")]
        [Permission("QuestionGroups.Delete")]
        public async Task<IActionResult> Delete(
            Guid groupId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteQuestionGroupCommand
            {
                GroupId = groupId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{groupId:guid}/restore")]
        [Permission("QuestionGroups.Update")]
        public async Task<IActionResult> Restore(
    Guid groupId,
    CancellationToken cancellationToken)
        {
            var command = new RestoreQuestionGroupCommand
            {
                GroupId = groupId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}