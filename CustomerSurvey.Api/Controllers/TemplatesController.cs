using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.Templates;
using CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate;
using CustomerSurvey.Application.Features.Templates.Command.CreateTemplate;
using CustomerSurvey.Application.Features.Templates.Command.DeleteTemplate;
using CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions;
using CustomerSurvey.Application.Features.Templates.Command.RestoreTemplate;
using CustomerSurvey.Application.Features.Templates.Command.UpdateTemplate;
using CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails;
using CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection;
using CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection;
using CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/templates")]
    [Authorize]
    public sealed class TemplatesController : ControllerBase
    {
        private readonly ISender sender;

        public TemplatesController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("Templates.ViewAll")]
        public async Task<IActionResult> GetPaginated(
        [FromQuery] GetTemplatesPaginationQuery query,
        CancellationToken cancellationToken)
        {
            query ??= new GetTemplatesPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("selection")]
        [Permission("Templates.ViewSelection")]
        public async Task<IActionResult> GetSelection(
      CancellationToken cancellationToken)
        {
            var query = new GetTemplatesForSelectionQuery();

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{templateId:guid}")]
        [Permission("Templates.ViewDetails")]
        public async Task<IActionResult> GetById(
          Guid templateId,
          CancellationToken cancellationToken)
        {
            var query = new GetTemplateDetailsQuery
            {
                TemplateId = templateId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{templateId:guid}/questions-selection")]
        [Permission("Templates.AssignQuestions")]
        public async Task<IActionResult> GetQuestionsSelection(
    Guid templateId,
    CancellationToken cancellationToken)
        {
            var query = new GetTemplateQuestionsSelectionQuery
            {
                TemplateId = templateId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{templateId:guid}/questions")]
        [Permission("Templates.AssignQuestions")]
        public async Task<IActionResult> AssignQuestions(
    Guid templateId,
    [FromBody] AssignQuestionsToTemplateRequest request,
    CancellationToken cancellationToken)
        {
            var command = new AssignQuestionsToTemplateCommand
            {
                TemplateId = templateId,
                QuestionIds = request.QuestionIds
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("Templates.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateTemplateRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateTemplateCommand
            {
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Description = request.Description
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{templateId:guid}")]
        [Permission("Templates.Update")]
        public async Task<IActionResult> Update(
    Guid templateId,
    [FromBody] UpdateTemplateRequest request,
    CancellationToken cancellationToken)
        {
            var command = new UpdateTemplateCommand
            {
                TemplateId = templateId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Description = request.Description
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{templateId:guid}/restore")]
        [Permission("Templates.Update")]
        public async Task<IActionResult> Restore(
    Guid templateId,
    CancellationToken cancellationToken)
        {
            var command = new RestoreTemplateCommand
            {
                TemplateId = templateId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{templateId:guid}")]
        [Permission("Templates.Delete")]
        public async Task<IActionResult> Delete(
    Guid templateId,
    CancellationToken cancellationToken)
        {
            var command = new DeleteTemplateCommand
            {
                TemplateId = templateId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{templateId:guid}/question-conditions")]
        [Permission("Templates.ManageQuestionConditions")]
        public async Task<IActionResult> ManageQuestionConditions(
    Guid templateId,
    [FromBody] ManageTemplateQuestionConditionsRequest request,
    CancellationToken cancellationToken)
        {
            var command = new ManageTemplateQuestionConditionsCommand
            {
                TemplateId = templateId,
                Conditions = request.Conditions
                    .Select(x => new TemplateQuestionConditionCommandItem
                    {
                        ParentTemplateQuestionId = x.ParentTemplateQuestionId,
                        ChildTemplateQuestionId = x.ChildTemplateQuestionId,
                        TriggerType = x.TriggerType,
                        SelectedQuestionOptionId = x.SelectedQuestionOptionId,
                        TriggerValue = x.TriggerValue,
                        Order = x.Order
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }
    }
}