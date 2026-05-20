using BuildingBlock.Api;
using CustomerSurvey.Api.Attribute;
using CustomerSurvey.Api.Contracts.AnonymousTemplates;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponseDetails;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination;
using CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSurvey.Api.Controllers
{
    [ApiController]
    [Route("api/anonymous-templates")]
    [Authorize]
    public sealed class AnonymousTemplatesController : ControllerBase
    {
        private readonly ISender sender;

        public AnonymousTemplatesController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet]
        [Permission("AnonymousTemplates.ViewAll")]
        public async Task<IActionResult> GetPaginated(
            [FromQuery] GetAnonymousTemplatesPaginationQuery query,
            CancellationToken cancellationToken)
        {
            query ??= new GetAnonymousTemplatesPaginationQuery();
            query.SearchText ??= string.Empty;

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{anonymousTemplateId:guid}")]
        [Permission("AnonymousTemplates.ViewDetails")]
        public async Task<IActionResult> GetDetails(
            [FromRoute] Guid anonymousTemplateId,
            CancellationToken cancellationToken)
        {
            var query = new GetAnonymousTemplateDetailsQuery
            {
                AnonymousTemplateId = anonymousTemplateId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPost]
        [Permission("AnonymousTemplates.Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAnonymousTemplateRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateAnonymousTemplateCommand
            {
                Scope = request.Scope,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Description = request.Description,
                ActiveFrom = request.ActiveFrom,
                ExpireTo = request.ExpireTo,
                CustomInputs = request.CustomInputs?
                    .Select(x => new CreateAnonymousTemplateCustomInputCommandItem
                    {
                        Name = x.Name,
                        LabelEn = x.LabelEn,
                        LabelAr = x.LabelAr,
                        Type = x.Type,
                        IsRequired = x.IsRequired,
                        MinLength = x.MinLength,
                        MaxLength = x.MaxLength,
                        MinValue = x.MinValue,
                        MaxValue = x.MaxValue,
                        Order = x.Order
                    })
                    .ToArray()
                    ?? Array.Empty<CreateAnonymousTemplateCustomInputCommandItem>()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{anonymousTemplateId:guid}")]
        [Permission("AnonymousTemplates.Update")]
        public async Task<IActionResult> Update(
    [FromRoute] Guid anonymousTemplateId,
    [FromBody] UpdateAnonymousTemplateRequest request,
    CancellationToken cancellationToken)
        {
            var command = new UpdateAnonymousTemplateCommand
            {
                AnonymousTemplateId = anonymousTemplateId,
                NameEn = request.NameEn,
                NameAr = request.NameAr,
                Description = request.Description,
                ActiveFrom = request.ActiveFrom,
                ExpireTo = request.ExpireTo,
                CustomInputs = request.CustomInputs
                    .Select(x => new UpdateAnonymousTemplateCustomInputCommandItem
                    {
                        CustomInputId = x.CustomInputId,
                        Name = x.Name,
                        LabelEn = x.LabelEn,
                        LabelAr = x.LabelAr,
                        Type = x.Type,
                        IsRequired = x.IsRequired,
                        MinLength = x.MinLength,
                        MaxLength = x.MaxLength,
                        MinValue = x.MinValue,
                        MaxValue = x.MaxValue,
                        Order = x.Order
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpDelete("{anonymousTemplateId:guid}")]
        [Permission("AnonymousTemplates.Delete")]
        public async Task<IActionResult> Delete(
    [FromRoute] Guid anonymousTemplateId,
    CancellationToken cancellationToken)
        {
            var command = new DeleteAnonymousTemplateCommand
            {
                AnonymousTemplateId = anonymousTemplateId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{anonymousTemplateId:guid}/restore")]
        [Permission("AnonymousTemplates.Restore")]
        public async Task<IActionResult> Restore(
            [FromRoute] Guid anonymousTemplateId,
            CancellationToken cancellationToken)
        {
            var command = new RestoreAnonymousTemplateCommand
            {
                AnonymousTemplateId = anonymousTemplateId
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{anonymousTemplateId:guid}/questions-selection")]
        [Permission("AnonymousTemplates.AssignQuestions")]
        public async Task<IActionResult> GetQuestionsSelection(
    [FromRoute] Guid anonymousTemplateId,
    [FromQuery] string? searchText,
    CancellationToken cancellationToken)
        {
            var query = new GetAnonymousTemplateQuestionsSelectionQuery
            {
                AnonymousTemplateId = anonymousTemplateId,
                SearchText = searchText
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{anonymousTemplateId:guid}/questions")]
        [Permission("AnonymousTemplates.AssignQuestions")]
        public async Task<IActionResult> AssignQuestions(
    [FromRoute] Guid anonymousTemplateId,
    [FromBody] AssignQuestionsToAnonymousTemplateRequest request,
    CancellationToken cancellationToken)
        {
            var command = new AssignQuestionsToAnonymousTemplateCommand
            {
                AnonymousTemplateId = anonymousTemplateId,
                Questions = request.Questions
                    .Select(x => new AssignQuestionToAnonymousTemplateCommandItem
                    {
                        QuestionId = x.QuestionId,
                        Order = x.Order
                    })
                    .ToArray()
            };

            var result = await sender.Send(command, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpPut("{anonymousTemplateId:guid}/question-conditions")]
        [Permission("AnonymousTemplates.ManageQuestionConditions")]
        public async Task<IActionResult> ManageQuestionConditions(
    [FromRoute] Guid anonymousTemplateId,
    [FromBody] ManageAnonymousTemplateQuestionConditionsRequest request,
    CancellationToken cancellationToken)
        {
            var command = new ManageAnonymousTemplateQuestionConditionsCommand
            {
                AnonymousTemplateId = anonymousTemplateId,
                Conditions = request.Conditions
                    .Select(x => new ManageAnonymousTemplateQuestionConditionCommandItem
                    {
                        ParentAnonymousTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
                        ChildAnonymousTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
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

        [HttpGet("{anonymousTemplateId:guid}/responses")]
        [Permission("AnonymousTemplates.ViewResponses")]
        public async Task<IActionResult> GetResponses(
    [FromRoute] Guid anonymousTemplateId,
    [FromQuery] GetAnonymousTemplateResponsesPaginationQuery query,
    CancellationToken cancellationToken)
        {
            query ??= new GetAnonymousTemplateResponsesPaginationQuery();
            query.SearchText ??= string.Empty;

            var request = new GetAnonymousTemplateResponsesPaginationQuery
            {
                AnonymousTemplateId = anonymousTemplateId,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchText = query.SearchText,
                OrderSort = query.OrderSort,
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                MinScorePercentage = query.MinScorePercentage,
                MaxScorePercentage = query.MaxScorePercentage
            };

            var result = await sender.Send(request, cancellationToken);

            return result.ToIActionResult();
        }

        [HttpGet("{anonymousTemplateId:guid}/responses/{responseId:guid}")]
        [Permission("AnonymousTemplates.ViewResponses")]
        public async Task<IActionResult> GetResponseDetails(
    [FromRoute] Guid anonymousTemplateId,
    [FromRoute] Guid responseId,
    CancellationToken cancellationToken)
        {
            var query = new GetAnonymousTemplateResponseDetailsQuery
            {
                AnonymousTemplateId = anonymousTemplateId,
                ResponseId = responseId
            };

            var result = await sender.Send(query, cancellationToken);

            return result.ToIActionResult();
        }
    }
}