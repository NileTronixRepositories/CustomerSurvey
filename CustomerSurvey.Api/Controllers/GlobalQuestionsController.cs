using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.GlobalQuestions;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.CreateGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.UpdateGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.DeleteGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Command.RestoreGlobalQuestion;
using CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/global-questions")]
    [Authorize]
    public sealed class GlobalQuestionsController : ControllerBase
    {
        private readonly ISender sender;

        public GlobalQuestionsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("GlobalQuestions.ViewAll")]
        public async Task<IActionResult> GetPaginated(
         [FromQuery] GetGlobalQuestionsPaginationQuery query,
         CancellationToken cancellationToken)
        {
            query ??= new GetGlobalQuestionsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{questionId:guid}")]
        [Permission("GlobalQuestions.ViewAll")]
        public async Task<IActionResult> GetById(
    Guid questionId,
    CancellationToken cancellationToken)
        {
            var query = new GetGlobalQuestionDetailsQuery
            {
                QuestionId = questionId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("GlobalQuestions.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateGlobalQuestionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateGlobalQuestionCommand
            {
                GroupId = request.GroupId,
                TextEn = request.TextEn,
                TextAr = request.TextAr,
                Type = request.Type,
                Options = request.Options
                    .Select(option => new CreateGlobalQuestionOptionCommandItem
                    {
                        TextEn = option.TextEn,
                        TextAr = option.TextAr,
                        Order = option.Order,
                        Value = option.Value
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{questionId:guid}")]
        [Permission("GlobalQuestions.Update")]
        public async Task<IActionResult> Update(
    Guid questionId,
    [FromBody] UpdateGlobalQuestionRequest request,
    CancellationToken cancellationToken)
        {
            var command = new UpdateGlobalQuestionCommand
            {
                QuestionId = questionId,
                GroupId = request.GroupId,
                TextEn = request.TextEn,
                TextAr = request.TextAr,
                Type = request.Type,
                Options = request.Options
                    .Select(option => new UpdateGlobalQuestionOptionCommandItem
                    {
                        OptionId = option.OptionId,
                        TextEn = option.TextEn,
                        TextAr = option.TextAr,
                        Order = option.Order,
                        Value = option.Value
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{questionId:guid}/restore")]
        [Permission("GlobalQuestions.Restore")]
        public async Task<IActionResult> Restore(
    Guid questionId,
    CancellationToken cancellationToken)
        {
            var command = new RestoreGlobalQuestionCommand
            {
                QuestionId = questionId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{questionId:guid}")]
        [Permission("GlobalQuestions.Delete")]
        public async Task<IActionResult> Delete(
    Guid questionId,
    CancellationToken cancellationToken)
        {
            var command = new DeleteGlobalQuestionCommand
            {
                QuestionId = questionId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}