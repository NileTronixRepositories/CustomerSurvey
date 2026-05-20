using BuildingBlock.Api;
using CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerSurvey.Api.Contracts.AnonTemplates;
using CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/anon-templates")]
    [AllowAnonymous]
    public sealed class AnonTemplatesController : ControllerBase
    {
        private readonly ISender sender;

        public AnonTemplatesController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("{anonymousTemplateId:guid}")]
        public async Task<IActionResult> GetPublicTemplate(
            [FromRoute] Guid anonymousTemplateId,
            CancellationToken cancellationToken)
        {
            var query = new GetPublicAnonymousTemplateQuery
            {
                AnonymousTemplateId = anonymousTemplateId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost("{anonymousTemplateId:guid}/responses")]
        public async Task<IActionResult> SubmitResponse(
    [FromRoute] Guid anonymousTemplateId,
    [FromBody] SubmitAnonymousTemplateResponseRequest request,
    CancellationToken cancellationToken)
        {
            var command = new SubmitAnonymousTemplateResponseCommand
            {
                AnonymousTemplateId = anonymousTemplateId,
                CustomInputValues = request.CustomInputValues
                    .Select(x => new SubmitAnonymousTemplateCustomInputValueCommandItem
                    {
                        CustomInputId = x.CustomInputId,
                        StringValue = x.StringValue,
                        IntegerValue = x.IntegerValue
                    })
                    .ToArray(),
                Answers = request.Answers
                    .Select(x => new SubmitAnonymousTemplateAnswerCommandItem
                    {
                        AnonymousTemplateQuestionId = x.AnonymousTemplateQuestionId,
                        SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                        StarRatingValue = x.StarRatingValue,
                        SmileValue = x.SmileValue,
                        TextAnswer = x.TextAnswer,
                        VoiceFileName = x.VoiceFileName
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}