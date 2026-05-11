using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Questions;
using CustomerSurvey.Application.Features.Questions.Command.CreateQuestion;
using CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion;
using CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion;
using CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/questions")]
    [Authorize]
    public sealed class QuestionsController : ControllerBase
    {
        private readonly ISender sender;

        public QuestionsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("Questions.ViewAll")]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] GetQuestionsPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetQuestionsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("Questions.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateQuestionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateQuestionCommand
            {
                GroupId = request.GroupId,
                TextEn = request.TextEn,
                TextAr = request.TextAr,
                Type = request.Type
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{questionId:guid}")]
        [Permission("Questions.Update")]
        public async Task<IActionResult> Update(
            Guid questionId,
            [FromBody] UpdateQuestionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateQuestionCommand
            {
                QuestionId = questionId,
                GroupId = request.GroupId,
                TextEn = request.TextEn,
                TextAr = request.TextAr,
                Type = request.Type
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{questionId:guid}")]
        [Permission("Questions.Delete")]
        public async Task<IActionResult> Delete(
            Guid questionId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteQuestionCommand
            {
                QuestionId = questionId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}