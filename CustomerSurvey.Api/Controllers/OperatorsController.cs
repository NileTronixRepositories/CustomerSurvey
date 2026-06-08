using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Operators;
using CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator;
using CustomerSurvey.Application.Features.Operators.Command.CreateOperator;
using CustomerSurvey.Application.Features.Operators.Command.UpdateOperator;
using CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates;
using CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination;
using CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection;
using CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/operators")]
    [Authorize]
    public sealed class OperatorsController : ControllerBase
    {
        private readonly ISender sender;

        public OperatorsController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("Operators.ViewAll")]
        public async Task<IActionResult> GetPaginated(
         [FromQuery] GetOperatorsPaginationQuery query,
         CancellationToken cancellationToken)
        {
            query ??= new GetOperatorsPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("Operators.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOperatorRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateOperatorCommand
            {
                DepartmentId = request.DepartmentId,
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

        [HttpGet("{operatorId:guid}/templates-selection")]
        [Permission("Operators.AssignTemplates")]
        public async Task<IActionResult> GetTemplatesSelection(
    Guid operatorId,
    [FromQuery] GetOperatorTemplatesSelectionQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetOperatorTemplatesSelectionQuery();

            var finalQuery = query with
            {
                OperatorId = operatorId,
                SearchText = query.SearchText ?? string.Empty
            };

            var result = await sender.Send(finalQuery, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{operatorId:guid}")]
        [Permission("Operators.Update")]
        public async Task<IActionResult> Update(
    Guid operatorId,
    [FromBody] UpdateOperatorRequest request,
    CancellationToken cancellationToken)
        {
            var command = new UpdateOperatorCommand
            {
                OperatorId = operatorId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{operatorId:guid}/templates")]
        [Permission("Operators.AssignTemplates")]
        public async Task<IActionResult> AssignTemplates(
    Guid operatorId,
    [FromBody] AssignTemplatesToOperatorRequest request,
    CancellationToken cancellationToken)
        {
            var command = new AssignTemplatesToOperatorCommand
            {
                OperatorId = operatorId,
                TemplateIds = request.TemplateIds
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("my-templates")]
        [Permission("OperatorTemplates.ViewMine")]
        public async Task<IActionResult> GetMyTemplates(
    CancellationToken cancellationToken)
        {
            var query = new GetMyOperatorTemplatesQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost("my-templates/{templateId:guid}/responses")]
        [Permission("OperatorTemplates.SubmitResponse")]
        public async Task<IActionResult> SubmitTemplateResponse(
     Guid templateId,
     [FromForm] SubmitOperatorTemplateResponseRequest request,
     CancellationToken cancellationToken)
        {
            var command = new SubmitOperatorTemplateResponseCommand
            {
                TemplateId = templateId,

                CustomInputs = (request.CustomInputs ?? new List<SubmitOperatorTemplateCustomInputRequest>())
                    .Select(x => new SubmitOperatorTemplateCustomInputCommandItem
                    {
                        CustomInputId = x.CustomInputId,
                        Value = x.Value
                    })
                    .ToArray(),

                Answers = (request.Answers ?? new List<SubmitOperatorTemplateAnswerRequest>())
                    .Select(x => new SubmitOperatorTemplateAnswerCommandItem
                    {
                        QuestionId = x.QuestionId,
                        SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                        StarRatingValue = x.StarRatingValue,
                        SmileValue = x.SmileValue,
                        TextAnswer = x.TextAnswer,
                        VoiceFile = x.VoiceFile,
                        ImageFile = x.ImageFile
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}
